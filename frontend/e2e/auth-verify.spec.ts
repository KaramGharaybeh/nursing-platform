// SCN-AUTH-VERIFY-001 — Email verification one-shot (UC-AUTH-04). Layer: UI journey.
import { expect, test } from '@playwright/test';
import { apiPost, ephemeralNurse, fetchEmailToken } from './fixtures';

test('fresh token verifies the account and enables login', async ({ page, request }) => {
  const user = ephemeralNurse();
  const signup = await apiPost(request, '/api/v1/auth/sign-up', {
    email: user.email,
    username: user.username,
    password: user.password,
  });
  expect(signup.status()).toBe(202);
  const token = await fetchEmailToken(request, user.email, 'verify');

  await page.goto(`/auth/verify-email/confirm?token=${encodeURIComponent(token)}`);
  await expect(page.locator('.np-verify-email-success')).toBeVisible();

  const login = await apiPost(request, '/api/v1/auth/login', {
    email: user.email,
    password: user.password,
  });
  expect(login.status()).toBe(200);
});

test('replayed and garbage tokens fail safely', async ({ page, request }) => {
  const user = ephemeralNurse();
  await apiPost(request, '/api/v1/auth/sign-up', {
    email: user.email,
    username: user.username,
    password: user.password,
  });
  const token = await fetchEmailToken(request, user.email, 'verify');
  await page.goto(`/auth/verify-email/confirm?token=${encodeURIComponent(token)}`);
  await expect(page.locator('.np-verify-email-success')).toBeVisible();

  await page.goto(`/auth/verify-email/confirm?token=${encodeURIComponent(token)}`);
  await expect(page.locator('.np-verify-email-error')).toBeVisible();

  await page.goto('/auth/verify-email/confirm?token=garbage-token-value');
  await expect(page.locator('.np-verify-email-error')).toBeVisible();
});
