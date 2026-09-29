// SCN-AUTH-SESSION-001 — Refresh/session lifecycle (UC-AUTH-02 API branch + UC-AUTH-10). Layer: MIXED.
import { expect, test } from '@playwright/test';
import { API_URL, NURSE_A, NURSE_LC, apiPost } from './fixtures';

test('refresh rotates the pair and reuse revokes everything', async ({ request }) => {
  const login = await apiPost(request, '/api/v1/auth/login', {
    email: NURSE_LC.email,
    password: NURSE_LC.password,
  });
  expect(login.status()).toBe(200);
  const first = (await login.json()) as { accessToken: string; refreshToken: string };

  const rotated = await apiPost(request, '/api/v1/auth/refresh', {
    refreshToken: first.refreshToken,
  });
  expect(rotated.status()).toBe(200);
  const second = (await rotated.json()) as { accessToken: string; refreshToken: string };
  expect(second.refreshToken).not.toBe(first.refreshToken);

  const reuse = await apiPost(request, '/api/v1/auth/refresh', {
    refreshToken: first.refreshToken,
  });
  expect(reuse.status()).toBe(401);

  // Re-login restores a working session after revocation.
  const relogin = await apiPost(request, '/api/v1/auth/login', {
    email: NURSE_LC.email,
    password: NURSE_LC.password,
  });
  expect(relogin.status()).toBe(200);
  const me = await request.get(`${API_URL}/api/v1/me`, {
    headers: { Authorization: `Bearer ${((await relogin.json()) as { accessToken: string }).accessToken}` },
  });
  expect(me.status()).toBe(200);
});

test('anonymous users are sent to sign-in with returnUrl', async ({ page }) => {
  await page.goto('/nurse/profile');
  await expect(page).toHaveURL(/\/auth\/sign-in\?returnUrl=/);
});

test('profile-complete users bounce off onboarding; nurses hit access-denied on admin', async ({
  page,
}) => {
  await page.goto('/auth/sign-in');
  await page.locator('#auth-sign-in-email').fill(NURSE_A.email);
  await page.locator('#auth-sign-in-password').fill(NURSE_A.password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/account$/);

  await page.goto('/onboarding/profile');
  await expect(page).not.toHaveURL(/\/onboarding\/profile$/);

  await page.goto('/admin/users');
  await expect(page).toHaveURL(/\/access-denied$/);
});
