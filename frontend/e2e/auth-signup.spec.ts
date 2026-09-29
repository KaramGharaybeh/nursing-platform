// SCN-AUTH-SIGNUP-001 — Public nurse sign-up (UC-AUTH-03). Layer: UI journey.
import { expect, test } from '@playwright/test';
import { NURSE_A, apiPost, ephemeralNurse } from './fixtures';

test('sign-up creates a nurse account and lands on the verify notice', async ({
  page,
  request,
}) => {
  const user = ephemeralNurse();
  await page.goto('/auth/sign-up');
  await page.locator('#auth-sign-up-email').fill(user.email);
  await page.locator('#auth-sign-up-username').fill(user.username);
  await page.locator('#auth-sign-up-password').fill(user.password);
  await page.locator('#auth-sign-up-confirm-password').fill(user.password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/auth\/verify-email$/);
  await expect(page.locator('#auth-check-email-title')).toBeVisible();
});

test('weak input is blocked client-side with a validation summary', async ({ page }) => {
  await page.goto('/auth/sign-up');
  await page.locator('#auth-sign-up-email').fill('not-an-email');
  await page.locator('#auth-sign-up-password').fill('short');
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/auth\/sign-up$/);
});

test('duplicate email returns 202 without mutating the existing account', async ({
  request,
}) => {
  const dup = await apiPost(request, '/api/v1/auth/sign-up', {
    email: NURSE_A.email,
    username: `test_dup_${Date.now().toString(36)}`,
    password: 'Test-Dup-01X',
  });
  expect(dup.status()).toBe(202);
  // Original credential still works: no overwrite happened.
  const login = await apiPost(request, '/api/v1/auth/login', {
    email: NURSE_A.email,
    password: NURSE_A.password,
  });
  expect(login.status()).toBe(200);
});

test('retired role-specific registration is gone', async ({ request }) => {
  for (const route of ['/api/v1/auth/register/nurse', '/api/v1/auth/register/employer']) {
    const res = await apiPost(request, route, {});
    expect(res.status()).toBe(410);
  }
});
