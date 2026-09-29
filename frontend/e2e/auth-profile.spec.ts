// SCN-AUTH-PROFILE-001 — View/update + forced onboarding (UC-AUTH-08). Layer: UI journey.
import { expect, test } from '@playwright/test';
import { API_URL, NURSE_A, ephemeralNurse, fetchEmailToken } from './fixtures';

async function uiLoginAs(page: Parameters<Parameters<typeof test>[1]>[0]['page'], email: string, password: string) {
  await page.goto('/auth/sign-in');
  await page.locator('#auth-sign-in-email').fill(email);
  await page.locator('#auth-sign-in-password').fill(password);
  await page.locator('button[type="submit"]').click();
}

test('account edits persist; fresh incomplete users are forced through onboarding', async ({
  page,
  request,
}) => {
  // NURSE_A edit → save → persisted (UI + API), then restored.
  await uiLoginAs(page, NURSE_A.email, NURSE_A.password);
  await expect(page).toHaveURL(/\/account$/);
  await page.locator('[data-testid="edit-personal-details"]').click();
  await page.locator('#account-personal-details-first-name').fill('TestA-Edited');
  await page.locator('#account-personal-details-last-name').fill('NurseA-Edited');
  await page.locator('button[type="submit"]').click();
  await expect(page.locator('[data-testid="edit-personal-details"]')).toBeVisible();

  await page.locator('[data-testid="edit-personal-details"]').click();
  await page.locator('#account-personal-details-first-name').fill(NURSE_A.firstName);
  await page.locator('#account-personal-details-last-name').fill(NURSE_A.lastName);
  await page.locator('button[type="submit"]').click();
  await expect(page.locator('[data-testid="edit-personal-details"]')).toBeVisible();

  // Fresh verified-but-nameless users are forced to onboarding on guarded routes.
  // (UI validators forbid blanking names, so clearing is not a real path to incomplete.)
  const fresh = ephemeralNurse();
  await request.post(`${API_URL}/api/v1/auth/sign-up`, {
    data: { email: fresh.email, username: fresh.username, password: fresh.password },
  });
  const freshToken = await fetchEmailToken(request, fresh.email, 'verify');
  await request.post(`${API_URL}/api/v1/auth/verify-email`, { data: { token: freshToken } });

  await uiLoginAs(page, fresh.email, fresh.password);
  await expect(page).toHaveURL(/\/onboarding\/profile/);
  await page.locator('#onboarding-profile-first-name').fill('Fresh');
  await page.locator('#onboarding-profile-last-name').fill('Nurse');
  await page.locator('button[type="submit"]').click();
  await expect(page).not.toHaveURL(/\/onboarding\/profile$/);

  // NURSE_A baseline intact (API proof).
  const login = await request.post(`${API_URL}/api/v1/auth/login`, {
    data: { email: NURSE_A.email, password: NURSE_A.password },
  });
  const token = ((await login.json()) as { accessToken: string }).accessToken;
  const me = await request.get(`${API_URL}/api/v1/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  const body = (await me.json()) as { firstName: string; lastName: string; isProfileComplete: boolean };
  expect(body.firstName).toBe(NURSE_A.firstName);
  expect(body.lastName).toBe(NURSE_A.lastName);
  expect(body.isProfileComplete).toBe(true);
});
