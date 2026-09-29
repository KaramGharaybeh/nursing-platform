#!/usr/bin/env bash
#
# Step 9 Tranche 1 — AUTH E2E orchestration (disposable Local/Test stack only).
# Fail-closed: refuses unless NURSING_E2E_ENABLED=1, env is Test/Development,
# and every target is loopback + run-scoped disposable (see e2e/test-env-guard.mjs).
# Starts: Postgres container (disposable DB) -> backend (Test env, self-migrates+
# seeds on boot) -> frontend (ng serve :4300, e2e proxy -> :5267) -> playwright.
# Tears everything it started down on exit. Never touches ports 5167/5432/4200.
set -u

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FE="$ROOT/frontend"
RUN_TAG="${NURSING_E2E_RUN_TAG:-$(head -c 6 /dev/urandom | od -An -tx1 | tr -d ' \n')}"

API_PORT=5267
WEB_PORT=4300
PG_HOST_PORT=55432
MP_SMTP_PORT=1125
MP_HTTP_PORT=8026
DB_NAME="nursing_auth_e2e_${RUN_TAG}"
DB_USER="e2e_auth"
DB_PASS="e2e-auth-test-only"
PG_CONTAINER="nursing-pg-auth-${RUN_TAG}"
MP_CONTAINER="nursing-mailpit-auth-${RUN_TAG}"
export NURSING_E2E_RUN_TAG="$RUN_TAG"

BACKEND_PID=""; FRONTEND_PID=""

fail() { echo "E2E REFUSED/FAILED: $1" >&2; exit 1; }

cleanup() {
  echo "== teardown =="
  [ -n "$FRONTEND_PID" ] && kill "$FRONTEND_PID" 2>/dev/null
  [ -n "$BACKEND_PID" ] && kill "$BACKEND_PID" 2>/dev/null
  wait 2>/dev/null
  docker rm -f "$PG_CONTAINER" >/dev/null 2>&1
  docker rm -f "$MP_CONTAINER" >/dev/null 2>&1
  echo "disposable containers removed"
}
trap cleanup EXIT INT TERM

echo "== Step 0: fail-closed guard =="
[ "${NURSING_E2E_ENABLED:-}" = "1" ] || fail "NURSING_E2E_ENABLED=1 not set"
export NURSING_E2E_ENV="Test"
export NURSING_E2E_APP_URL="http://localhost:${WEB_PORT}"
export NURSING_E2E_API_URL="http://localhost:${API_PORT}"
export NURSING_E2E_MAILPIT_URL="http://localhost:${MP_HTTP_PORT}"
export NURSING_E2E_PG_CONN="Host=localhost;Port=${PG_HOST_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}"
node --input-type=module -e "
import { resolveGuard } from '$FE/e2e/test-env-guard.mjs';
const r = resolveGuard({
  env: process.env.NURSING_E2E_ENV,
  optIn: process.env.NURSING_E2E_ENABLED,
  connectionString: process.env.NURSING_E2E_PG_CONN,
  appUrl: process.env.NURSING_E2E_APP_URL,
  apiUrl: process.env.NURSING_E2E_API_URL,
});
if (!r.ok) { console.error(r.reasons.join('\n')); process.exit(1); }
console.log('guard: PASS (disposable Test target verified)');
" || fail "environment guard refused"

echo "== Step 1: disposable Postgres + SMTP catcher =="
docker run -d --rm --name "$PG_CONTAINER" \
  -e POSTGRES_USER="$DB_USER" -e POSTGRES_PASSWORD="$DB_PASS" -e POSTGRES_DB="$DB_NAME" \
  -p "127.0.0.1:${PG_HOST_PORT}:5432" postgres:17 >/dev/null || fail "pg container"
docker run -d --rm --name "$MP_CONTAINER" \
  -p "127.0.0.1:${MP_SMTP_PORT}:1025" -p "127.0.0.1:${MP_HTTP_PORT}:8025" \
  axllent/mailpit:latest >/dev/null || fail "mailpit container"
for i in $(seq 1 30); do
  docker exec "$PG_CONTAINER" pg_isready -U "$DB_USER" >/dev/null 2>&1 && break
  sleep 1
done
docker exec "$PG_CONTAINER" pg_isready -U "$DB_USER" >/dev/null 2>&1 || fail "pg never ready"

echo "== Step 2: backend (Test env, disposable DB) =="
cd "$ROOT"
# shellcheck disable=SC2086
ASPNETCORE_ENVIRONMENT=Test \
NURSING_E2E_ENABLED=1 \
Database__ConnectionString="Host=localhost;Port=${PG_HOST_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}" \
Redis__ConnectionString="" \
Jwt__Secret="test-only-e2e-signing-key-minimum-32-characters-0000" \
Jwt__Issuer="NursingPlatformTest" Jwt__Audience="NursingPlatformTest" \
Jwt__ExpirationInMinutes=60 Jwt__RefreshTokenExpirationInDays=7 \
Admin__Email="admin@test.nursing-platform.test" Admin__Password="Test-Admin-A-06" \
Admin__FirstName="Test" Admin__LastName="Admin" \
Email__ApplicationUrl="http://localhost:${WEB_PORT}" Email__SmtpHost="localhost" \
Email__SmtpPort="${MP_SMTP_PORT}" Email__Username="" Email__Password="" \
Email__FromAddress="noreply@test.nursing-platform.test" Email__FromName="Nursing Platform Test" \
Email__UseSsl=false FileStorage__RootPath="/tmp/nursing-platform-e2e-auth-${RUN_TAG}" \
  dotnet run --project backend/src/NursingPlatform.WebApi/NursingPlatform.WebApi.csproj \
  --urls "http://localhost:${API_PORT}" --no-launch-profile >"/tmp/nursing-e2e-backend-${RUN_TAG}.log" 2>&1 &
BACKEND_PID=$!
for i in $(seq 1 120); do
  curl -sf "http://localhost:${API_PORT}/health/ready" >/dev/null 2>&1 && break
  sleep 2
done
curl -sf "http://localhost:${API_PORT}/health/ready" >/dev/null 2>&1 || fail "backend never ready (see /tmp/nursing-e2e-backend-${RUN_TAG}.log)"
echo "backend ready"

echo "== Step 3: frontend (:${WEB_PORT} -> :${API_PORT}) =="
cd "$FE"
npx ng serve --port "$WEB_PORT" --proxy-config e2e/proxy.e2e.json >"/tmp/nursing-e2e-frontend-${RUN_TAG}.log" 2>&1 &
FRONTEND_PID=$!
for i in $(seq 1 120); do
  curl -sf "http://localhost:${WEB_PORT}/auth/sign-in" >/dev/null 2>&1 && break
  sleep 2
done
curl -sf "http://localhost:${WEB_PORT}/auth/sign-in" >/dev/null 2>&1 || fail "frontend never ready (see /tmp/nursing-e2e-frontend-${RUN_TAG}.log)"
echo "frontend ready"

echo "== Step 4: playwright AUTH suite =="
npx playwright test --config playwright.config.ts "$@"
