// Step 9 Tranche 1 — fail-closed test environment guard.
// Current owners: docs/testing/test-data-catalog.md and docs/operations/environments.md.
// Pure functions only (no I/O, no secrets in outputs). Used by global-setup and
// the orchestration script BEFORE any migration, seeding, or provisioning.

const DISPOSABLE_DB_PATTERN = /^nursing_auth_e2e_[a-z0-9]{6,24}$/;
const ALLOWED_ENVS = new Set(['development', 'test']);
const REFUSED_ENVS = new Set(['production', 'staging']);

/** Run-scoped disposable database names only; rejects dev/real names and injection shapes. */
export function isDisposableDatabaseName(name) {
  return typeof name === 'string' && DISPOSABLE_DB_PATTERN.test(name);
}

/** Loopback HTTP(S) origins only. */
export function isLocalhostTarget(url) {
  try {
    const host = new URL(url).hostname.toLowerCase();
    return host === 'localhost' || host === '127.0.0.1' || host === '::1';
  } catch {
    return false;
  }
}

function parseNpgsqlConnectionString(value) {
  const out = {};
  if (typeof value !== 'string') return out;
  for (const part of value.split(';')) {
    const idx = part.indexOf('=');
    if (idx < 0) continue;
    out[part.slice(0, idx).trim().toLowerCase()] = part.slice(idx + 1).trim();
  }
  return out;
}

/**
 * Fail-closed gate. Input: { env, optIn, connectionString, appUrl, apiUrl }.
 * Returns { ok, reasons[] } — reasons NEVER contain secret values.
 * ok === true only when: known Test/Dev env + explicit opt-in + localhost app/api
 * + localhost DB host + disposable DB name.
 */
export function resolveGuard(input) {
  const reasons = [];
  const env = String(input?.env ?? '');
  const envLower = env.toLowerCase();

  if (REFUSED_ENVS.has(envLower)) {
    reasons.push(`refused environment: ${envLower} targets are never provisioned`);
  } else if (!ALLOWED_ENVS.has(envLower)) {
    reasons.push('refused environment: unknown or empty (expected Development or Test)');
  }
  if (String(input?.optIn ?? '') !== '1') {
    reasons.push('refused: explicit test-harness opt-in marker missing');
  }
  if (!isLocalhostTarget(String(input?.appUrl ?? ''))) {
    reasons.push('refused: app URL is not an explicit loopback origin');
  }
  if (!isLocalhostTarget(String(input?.apiUrl ?? ''))) {
    reasons.push('refused: api URL is not an explicit loopback origin');
  }

  const kv = parseNpgsqlConnectionString(input?.connectionString);
  const host = (kv.host ?? kv.server ?? '').toLowerCase();
  if (host !== 'localhost' && host !== '127.0.0.1') {
    reasons.push('refused: database host is not loopback');
  }
  if (!isDisposableDatabaseName(kv.database ?? '')) {
    reasons.push('refused: database name is not a run-scoped disposable test database');
  }

  return { ok: reasons.length === 0, reasons };
}
