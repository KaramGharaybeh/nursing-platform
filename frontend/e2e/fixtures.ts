// Step 9 Tranche 1 shared E2E helpers (AUTH family).
// Test-data owner: docs/testing/test-data-catalog.md (single source here;
// specs reference these constants, never password literals).
// Token retrieval: local MailPit HTTP API (test mechanism only, never UI scraping,
// never production mail). Secrets are never logged.
import type { APIRequestContext } from '@playwright/test';
import pg from 'pg';

export const API_URL = process.env['NURSING_E2E_API_URL'] ?? 'http://localhost:5267';
export const MAILPIT_URL = process.env['NURSING_E2E_MAILPIT_URL'] ?? 'http://localhost:8026';
const PG_CONN = process.env['NURSING_E2E_PG_CONN'] ?? '';

export const RUN_TAG =
  process.env['NURSING_E2E_RUN_TAG'] ?? Math.random().toString(16).slice(2, 14);

export const NURSE_A = {
  email: 'nurse.a@test.nursing-platform.test',
  username: 'test_nurse_a',
  password: 'Test-Nurse-A-01',
  firstName: 'TestA',
  lastName: 'NurseA',
} as const;

export const NURSE_LC = {
  email: 'nurse.lc@test.nursing-platform.test',
  username: 'test_nurse_lc',
  password: 'Test-Nurse-LC-03',
  firstName: 'TestLC',
  lastName: 'NurseLC',
} as const;

export function ephemeralNurse() {
  const tag = `${RUN_TAG}${Math.random().toString(16).slice(2, 8)}`;
  return {
    email: `lc-${tag}@test.nursing-platform.test`,
    username: `test_lc_${tag}`.slice(0, 50),
    password: 'Test-Lc-Run-07',
  };
}

export async function apiPost(
  request: APIRequestContext,
  path: string,
  body: unknown,
  token?: string,
) {
  const headers: Record<string, string> = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return request.post(`${API_URL}${path}`, { data: body, headers });
}

export async function apiPut(
  request: APIRequestContext,
  path: string,
  body: unknown,
  token?: string,
) {
  const headers: Record<string, string> = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return request.put(`${API_URL}${path}`, { data: body, headers });
}

export async function loginViaApi(
  request: APIRequestContext,
  email: string,
  password: string,
) {
  const res = await apiPost(request, '/api/v1/auth/login', { email, password });
  if (res.status() !== 200) {
    throw new Error(`login failed for ${email}: HTTP ${res.status()}`);
  }
  return (await res.json()) as { accessToken: string; refreshToken: string };
}

/** Poll MailPit API for a message to `recipient`, then extract the token link. */
export async function fetchEmailToken(
  request: APIRequestContext,
  recipient: string,
  kind: 'verify' | 'reset',
  timeoutMs = 60_000,
): Promise<string> {
  const deadline = Date.now() + timeoutMs;
  const slug = kind === 'verify' ? 'verify-email\\/confirm' : 'reset-password';
  const re = new RegExp(`${slug}\\?token=([^"\\s&<>]+)`);
  let last = 0;
  while (Date.now() < deadline) {
    const list = await request.get(`${MAILPIT_URL}/api/v1/messages?limit=50`);
    if (list.ok()) {
      const data = (await list.json()) as {
        messages?: Array<{ ID: string; To?: Array<{ Address: string }> }>;
        messages_count?: number;
      };
      for (const m of data.messages ?? []) {
        if (!(m.To ?? []).some((t) => t.Address === recipient)) continue;
        const full = await request.get(`${MAILPIT_URL}/api/v1/message/${m.ID}`);
        if (!full.ok()) continue;
        const body = (await full.json()) as { Text?: string; HTML?: string };
        // Undo quoted-printable soft line breaks before matching (long tokens wrap).
        const text = `${body.Text ?? ''}\n${(body.HTML ?? '').replace(/&amp;/g, '&')}`.replace(
          /=\r?\n/g,
          '',
        );
        const match = re.exec(text);
        if (match?.[1]) return decodeURIComponent(match[1]);
      }
      last = data.messages_count ?? last;
    }
    await new Promise((r) => setTimeout(r, 1500));
  }
  throw new Error(`no ${kind} email observed for ${recipient} (seen ${last} messages)`);
}

export async function mailCountFor(
  request: APIRequestContext,
  recipient: string,
): Promise<number> {
  const list = await request.get(`${MAILPIT_URL}/api/v1/messages?limit=100`);
  if (!list.ok()) return -1;
  const data = (await list.json()) as {
    messages?: Array<{ To?: Array<{ Address: string }> }>;
  };
  return (data.messages ?? []).filter((m) => (m.To ?? []).some((t) => t.Address === recipient))
    .length;
}

/** Provision a nurse end-to-end via real flows (signup → token → verify → names). */
export async function provisionNurseViaApi(
  request: APIRequestContext,
  user: { email: string; username: string; password: string; firstName: string; lastName: string },
) {
  const signup = await apiPost(request, '/api/v1/auth/sign-up', {
    email: user.email,
    username: user.username,
    password: user.password,
  });
  if (signup.status() !== 202) {
    throw new Error(`signup failed for ${user.email}: HTTP ${signup.status()}`);
  }
  const token = await fetchEmailToken(request, user.email, 'verify');
  const verify = await apiPost(request, '/api/v1/auth/verify-email', { token });
  if (verify.status() !== 200) {
    throw new Error(`verify failed for ${user.email}: HTTP ${verify.status()}`);
  }
  const session = await loginViaApi(request, user.email, user.password);
  const profile = await apiPut(
    request,
    '/api/v1/me/profile',
    { firstName: user.firstName, lastName: user.lastName },
    session.accessToken,
  );
  if (profile.status() !== 200) {
    throw new Error(`profile completion failed for ${user.email}: HTTP ${profile.status()}`);
  }
  return session;
}

/** Setup-synthesized IsActive flip on the disposable DB (STATE_INACTIVE_USER). */
export async function setUserActive(email: string, active: boolean): Promise<void> {
  if (!PG_CONN) throw new Error('NURSING_E2E_PG_CONN is not set');
  // node-postgres needs a URI; the harness carries Npgsql key=value shape (guard-checked).
  const kv: Record<string, string> = {};
  for (const part of PG_CONN.split(';')) {
    const idx = part.indexOf('=');
    if (idx > 0) kv[part.slice(0, idx).trim().toLowerCase()] = part.slice(idx + 1).trim();
  }
  const uri =
    `postgres://${encodeURIComponent(kv['username'] ?? '')}:${encodeURIComponent(kv['password'] ?? '')}` +
    `@${kv['host'] ?? 'localhost'}:${kv['port'] ?? '5432'}/${kv['database'] ?? ''}`;
  const client = new pg.Client({ connectionString: uri });
  await client.connect();
  try {
    const r = await client.query('UPDATE "Users" SET "IsActive" = $2 WHERE "Email" = $1', [email, active]);
    if (r.rowCount !== 1) throw new Error(`IsActive flip affected ${r.rowCount} rows for ${email}`);
  } finally {
    await client.end();
  }
}
