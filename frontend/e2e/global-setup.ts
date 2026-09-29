// Playwright global setup: fail-closed guard + canonical AUTH identity provisioning.
// Refuses (aborts the run) unless the Step 7 contract holds; provisions NURSE_A and
// NURSE_LC through real product flows on the disposable database.
import type { FullConfig } from '@playwright/test';
import { request } from '@playwright/test';
import { API_URL, NURSE_A, NURSE_LC, provisionNurseViaApi } from './fixtures';
import { resolveGuard } from './test-env-guard.mjs';

async function globalSetup(_config: FullConfig): Promise<void> {
  const verdict = resolveGuard({
    env: process.env['NURSING_E2E_ENV'] ?? process.env['ASPNETCORE_ENVIRONMENT'] ?? '',
    optIn: process.env['NURSING_E2E_ENABLED'] ?? '',
    connectionString: process.env['NURSING_E2E_PG_CONN'] ?? '',
    appUrl: process.env['NURSING_E2E_APP_URL'] ?? 'http://localhost:4300',
    apiUrl: API_URL,
  });
  if (!verdict.ok) {
    throw new Error(`E2E environment guard REFUSED:\n- ${verdict.reasons.join('\n- ')}`);
  }

  const ctx = await request.newContext({ baseURL: API_URL });
  try {
    const health = await ctx.get(`${API_URL}/health/ready`);
    if (!health.ok()) throw new Error(`backend not ready: HTTP ${health.status()}`);
    await provisionNurseViaApi(ctx, NURSE_A);
    await provisionNurseViaApi(ctx, NURSE_LC);
  } finally {
    await ctx.dispose();
  }
}

export default globalSetup;
