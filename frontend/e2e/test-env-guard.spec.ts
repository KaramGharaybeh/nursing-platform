// Step 9 Tranche 1 — fail-closed environment guard contract tests.
// Canonical: docs/testing/test-environment-provisioning-contract.md §4.
// Pure-logic tests for ../test-env-guard.mjs (TDD RED first).
import { describe, expect, it } from 'vitest';
import {
  isDisposableDatabaseName,
  isLocalhostTarget,
  resolveGuard,
} from './test-env-guard.mjs';

const GOOD_DB = 'Host=localhost;Port=55432;Database=nursing_auth_e2e_a1b2c3d4e5f6;Username=t;Password=x';

describe('isDisposableDatabaseName', () => {
  it('accepts the run-scoped disposable pattern', () => {
    expect(isDisposableDatabaseName('nursing_auth_e2e_a1b2c3d4e5f6')).toBe(true);
  });
  it('rejects the dev database name', () => {
    expect(isDisposableDatabaseName('nursing_platform')).toBe(false);
  });
  it('rejects empty and hostile shapes', () => {
    expect(isDisposableDatabaseName('')).toBe(false);
    expect(isDisposableDatabaseName('nursing_auth_e2e_;DROP TABLE users')).toBe(false);
    expect(isDisposableDatabaseName('postgres')).toBe(false);
  });
});

describe('isLocalhostTarget', () => {
  it('accepts loopback http origins', () => {
    expect(isLocalhostTarget('http://localhost:4300')).toBe(true);
    expect(isLocalhostTarget('http://127.0.0.1:5267/health')).toBe(true);
  });
  it('rejects remote/prod-like origins', () => {
    expect(isLocalhostTarget('https://staging.example.com')).toBe(false);
    expect(isLocalhostTarget('https://nursing.example.com')).toBe(false);
    expect(isLocalhostTarget('not-a-url')).toBe(false);
  });
});

describe('resolveGuard', () => {
  const base = {
    env: 'Test',
    optIn: '1',
    connectionString: GOOD_DB,
    appUrl: 'http://localhost:4300',
    apiUrl: 'http://localhost:5267',
  };
  it('passes for an authorized disposable Local/Test target', () => {
    const r = resolveGuard(base);
    expect(r.ok).toBe(true);
    expect(r.reasons).toEqual([]);
  });
  it('refuses Production even with opt-in', () => {
    const r = resolveGuard({ ...base, env: 'Production' });
    expect(r.ok).toBe(false);
    expect(r.reasons.join(' ')).toMatch(/production/i);
  });
  it('refuses Staging', () => {
    expect(resolveGuard({ ...base, env: 'Staging' }).ok).toBe(false);
  });
  it('refuses unknown/empty environment', () => {
    expect(resolveGuard({ ...base, env: '' }).ok).toBe(false);
    expect(resolveGuard({ ...base, env: 'UAT' }).ok).toBe(false);
  });
  it('refuses without explicit opt-in', () => {
    expect(resolveGuard({ ...base, optIn: '0' }).ok).toBe(false);
    expect(resolveGuard({ ...base, optIn: '' }).ok).toBe(false);
  });
  it('refuses a non-disposable database (dev name)', () => {
    const r = resolveGuard({
      ...base,
      connectionString: 'Host=localhost;Database=nursing_platform;Username=u;Password=p',
    });
    expect(r.ok).toBe(false);
  });
  it('refuses a remote database host', () => {
    const r = resolveGuard({
      ...base,
      connectionString: 'Host=db.internal;Database=nursing_auth_e2e_a1b2c3d4e5f6;Username=u;Password=p',
    });
    expect(r.ok).toBe(false);
  });
  it('refuses non-localhost app/api URLs', () => {
    expect(resolveGuard({ ...base, appUrl: 'https://staging.example.com' }).ok).toBe(false);
    expect(resolveGuard({ ...base, apiUrl: 'https://api.example.com' }).ok).toBe(false);
  });
  it('never echoes secrets in reasons', () => {
    const r = resolveGuard({ ...base, env: 'Production', connectionString: GOOD_DB });
    expect(r.reasons.join('\n')).not.toContain('Password=x');
    expect(r.reasons.join('\n')).not.toContain('a1b2c3d4e5f6');
  });
});
