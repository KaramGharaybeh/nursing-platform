// SCN-AUTH-PASSWORD-001 — Forgot/reset round-trip with canonical restore (UC-AUTH-06/07). Layer: UI journey.
import { expect, test, type Page } from '@playwright/test';
import { NURSE_LC, apiPost, fetchEmailToken } from './fixtures';

const AWAY_PASSWORD = 'Test-Nurse-LC-99';

async function forgotSubmit(page: Page, email: string) {
  await page.goto('/auth/forgot-password');
  await page.locator('#auth-forgot-password-email').fill(email);
  const [res] = await Promise.all([
    page.waitForResponse(
      (r) => r.url().includes('/api/v1/auth/forgot-password') && r.request().method() === 'POST',
    ),
    page.locator('button[type="submit"]').click(),
  ]);
  expect(res.status()).toBe(200);
  await expect(page.locator('.np-forgot-password-error')).toHaveCount(0);
}

async function resetSubmit(page: Page, email: string, token: string, newPassword: string) {
  await page.goto(`/auth/reset-password?token=${encodeURIComponent(token)}`);
  await page.locator('#auth-reset-password-email').fill(email);
  await page.locator('#auth-reset-password-new-password').fill(newPassword);
  const [res] = await Promise.all([
    page.waitForResponse(
      (r) => r.url().includes('/api/v1/auth/reset-password') && r.request().method() === 'POST',
    ),
    page.locator('button[type="submit"]').click(),
  ]);
  return res.status();
}

test('forgot is enumeration-safe; reset round-trips and restores canonical credential', async ({
  page,
  request,
}) => {
  // Known and unknown addresses produce the identical generic outcome.
  await forgotSubmit(page, NURSE_LC.email);
  await forgotSubmit(page, `unknown-${Date.now().toString(36)}@test.nursing-platform.test`);

  // Weak passwords are blocked client-side (no API call, stays on page).
  const token1 = await fetchEmailToken(request, NURSE_LC.email, 'reset');
  await page.goto(`/auth/reset-password?token=${encodeURIComponent(token1)}`);
  await page.locator('#auth-reset-password-email').fill(NURSE_LC.email);
  await page.locator('#auth-reset-password-new-password').fill('short');
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/auth\/reset-password/);

  // Away rotation: strict success requires the success panel, not just footer links.
  const token2 = await fetchEmailToken(request, NURSE_LC.email, 'reset');
  expect(await resetSubmit(page, NURSE_LC.email, token2, AWAY_PASSWORD)).toBe(200);
  await expect(page.locator('.np-reset-password-success')).toBeVisible();

  expect(
    (await apiPost(request, '/api/v1/auth/login', { email: NURSE_LC.email, password: NURSE_LC.password })).status(),
  ).toBe(401);
  expect(
    (await apiPost(request, '/api/v1/auth/login', { email: NURSE_LC.email, password: AWAY_PASSWORD })).status(),
  ).toBe(200);

  // Restore canonical credential (required before reuse) — needs a fresh token.
  await forgotSubmit(page, NURSE_LC.email);
  const token3 = await fetchEmailToken(request, NURSE_LC.email, 'reset');
  expect(await resetSubmit(page, NURSE_LC.email, token3, NURSE_LC.password)).toBe(200);
  await expect(page.locator('.np-reset-password-success')).toBeVisible();
  expect(
    (await apiPost(request, '/api/v1/auth/login', { email: NURSE_LC.email, password: NURSE_LC.password })).status(),
  ).toBe(200);
});

test('garbage reset tokens fail safely', async ({ page }) => {
  await page.goto('/auth/reset-password?token=garbage-token-value');
  await page.locator('#auth-reset-password-email').fill(NURSE_LC.email);
  await page.locator('#auth-reset-password-new-password').fill('Test-Nurse-LC-98');
  await page.locator('button[type="submit"]').click();
  await expect(page.locator('.np-reset-password-error')).toBeVisible();
});
