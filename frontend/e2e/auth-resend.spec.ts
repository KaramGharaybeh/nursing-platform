// SCN-AUTH-RESEND-001 — Resend verification, API-only by design (UC-AUTH-05). Layer: API_ONLY.
//
// Implementation note (verified against handlers): the real-send branch requires an
// authenticated-but-unverified caller, which current login rules make unreachable
// (correct-creds-unverified → 403, so no token can ever be held while unverified).
// The reachable contract is: verified → 200 without new mail; anonymous → 401.
// This spec pins exactly that reachable contract and nothing more.
import { expect, test } from '@playwright/test';
import { NURSE_LC, apiPost, loginViaApi, mailCountFor } from './fixtures';

test('verified accounts get success without a new email', async ({ request }) => {
  const session = await loginViaApi(request, NURSE_LC.email, NURSE_LC.password);
  const before = await mailCountFor(request, NURSE_LC.email);
  const res = await apiPost(request, '/api/v1/auth/send-verification-email', {}, session.accessToken);
  expect(res.status()).toBe(200);
  expect(await mailCountFor(request, NURSE_LC.email)).toBe(before);
});

test('anonymous resend is rejected', async ({ request }) => {
  const res = await apiPost(request, '/api/v1/auth/send-verification-email', {});
  expect(res.status()).toBe(401);
});
