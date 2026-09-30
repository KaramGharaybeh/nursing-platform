// SCN-AUTH-LOGIN-001 — Login matrix (UC-AUTH-01). Layer: UI journey.
import { expect, test } from '@playwright/test';
import { NURSE_A, apiPost, ephemeralNurse, fetchEmailToken, setUserActive } from './fixtures';

test('verified credentials sign in and land on the account page', async ({ page }) => {
  await page.goto('/auth/sign-in');
  await page.locator('#auth-sign-in-email').fill(NURSE_A.email);
  await page.locator('#auth-sign-in-password').fill(NURSE_A.password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/account$/);
  await expect(page.locator('#account-title')).toBeVisible();
  await expect(page.getByText(NURSE_A.email)).toBeVisible();
});

test('unverified credentials are rejected with a verify-required message', async ({
  page,
  request,
}) => {
  const user = ephemeralNurse();
  await apiPost(request, '/api/v1/auth/sign-up', {
    email: user.email,
    username: user.username,
    password: user.password,
  });
  await page.goto('/auth/sign-in');
  await page.locator('#auth-sign-in-email').fill(user.email);
  await page.locator('#auth-sign-in-password').fill(user.password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/auth\/sign-in$/);
  await expect(page.locator('.np-sign-in-error')).toBeVisible();
});

test('wrong password is rejected indistinguishably', async ({ page, request }) => {
  const apiRes = await apiPost(request, '/api/v1/auth/login', {
    email: NURSE_A.email,
    password: 'Wrong-Password-00',
  });
  expect(apiRes.status()).toBe(401);

  await page.goto('/auth/sign-in');
  await page.locator('#auth-sign-in-email').fill(NURSE_A.email);
  await page.locator('#auth-sign-in-password').fill('Wrong-Password-00');
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/auth\/sign-in$/);
  await expect(page.locator('.np-sign-in-error')).toBeVisible();
});

test('inactive account cannot sign in; reactivation restores access', async ({
  page,
  request,
}) => {
  const user = ephemeralNurse();
  await apiPost(request, '/api/v1/auth/sign-up', {
    email: user.email,
    username: user.username,
    password: user.password,
  });
  const token = await fetchEmailToken(request, user.email, 'verify');
  await apiPost(request, '/api/v1/auth/verify-email', { token });

  await setUserActive(user.email, false);
  try {
    await page.goto('/auth/sign-in');
    await page.locator('#auth-sign-in-email').fill(user.email);
    await page.locator('#auth-sign-in-password').fill(user.password);
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/\/auth\/sign-in$/);
    await expect(page.locator('.np-sign-in-error')).toBeVisible();
  } finally {
    await setUserActive(user.email, true);
  }

  const login = await apiPost(request, '/api/v1/auth/login', {
    email: user.email,
    password: user.password,
  });
  expect(login.status()).toBe(200);
});
