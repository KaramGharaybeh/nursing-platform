import { defineConfig } from '@playwright/test';

// Step 9 Tranche 1 — AUTH family only (testMatch). Infra (backend :5267,
// frontend :4300, disposable Postgres, local SMTP catcher) is started by
// e2e/run-auth-e2e.sh; this config assumes running services (no webServer)
// so orchestration stays explicit and debuggable.
export default defineConfig({
  testDir: './e2e',
  testMatch: ['auth-*.spec.ts'],
  globalSetup: './e2e/global-setup.ts',
  fullyParallel: false,
  workers: 1,
  timeout: 90_000,
  expect: { timeout: 15_000 },
  use: {
    baseURL: process.env['NURSING_E2E_APP_URL'] ?? 'http://localhost:4300',
    trace: 'retain-on-failure',
  },
  reporter: [['list']],
});
