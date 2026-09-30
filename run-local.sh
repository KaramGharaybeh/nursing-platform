#!/usr/bin/env bash
#
# run-local.sh — تشغيل nursing-platform محليًا (backend + frontend)
# من جذر المشروع:  ./run-local.sh
# الإيقاف:         Ctrl + C  (يوقف الخدمتين معًا)
#
# لا يثبّت حزمًا ولا يعدّل أي ملف في المشروع. يكتب ملفات log فقط تحت ./logs/

set -u

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="$ROOT/logs"
mkdir -p "$LOG_DIR"

BACKEND_URL="http://localhost:5167"
FRONTEND_URL="http://localhost:4200"
BACKEND_LOG="$LOG_DIR/backend.log"
FRONTEND_LOG="$LOG_DIR/frontend.log"

fail() { echo "❌ $1" >&2; exit 1; }

echo "== فحص المتطلبات (بدون تثبيت أي شيء) =="
command -v dotnet >/dev/null || fail "dotnet غير موجود في PATH"
command -v npm >/dev/null || fail "npm غير موجود في PATH"
command -v curl >/dev/null || fail "curl غير موجود في PATH"
[ -d "$ROOT/frontend/node_modules" ] || fail "مجلد frontend/node_modules غير موجود. نفّذ بنفسك داخل frontend:  npm ci"
(timeout 3 bash -c "</dev/tcp/localhost/5432") 2>/dev/null \
  || fail "PostgreSQL غير reachable على localhost:5432 (قاعدة nursing_platform). شغّل قاعدة التطوير أولًا."
echo "✔ المتطلبات موجودة"
echo "✔ logs تُحفظ في: $LOG_DIR"

cleanup() {
  echo ""
  echo "== إيقاف الخدمتين... =="
  kill "$BACKEND_PID" "$FRONTEND_PID" 2>/dev/null
  wait 2>/dev/null
  echo "✔ توقف الـbackend والـfrontend"
  exit 0
}
trap cleanup INT TERM

echo "== 1/2 تشغيل الـbackend ($BACKEND_URL) =="
dotnet run --project "$ROOT/backend/src/NursingPlatform.WebApi/NursingPlatform.WebApi.csproj" \
  --launch-profile http >"$BACKEND_LOG" 2>&1 &
BACKEND_PID=$!

echo "   انتظار جاهزية الـbackend..."
for _ in $(seq 1 30); do
  if curl -sf -o /dev/null "$BACKEND_URL/api/v1/preparation-packages/offers?page=1&pageSize=1"; then
    echo "✔ الـbackend جاهز"
    break
  fi
  sleep 3
  if ! kill -0 "$BACKEND_PID" 2>/dev/null; then
    fail "الـbackend توقف أثناء الإقلاع. راجع: $BACKEND_LOG"
  fi
done
curl -sf -o /dev/null "$BACKEND_URL/api/v1/preparation-packages/offers?page=1&pageSize=1" \
  || fail "الـbackend لم يجهز خلال المهلة. راجع: $BACKEND_LOG"

echo "== 2/2 تشغيل الـfrontend ($FRONTEND_URL) =="
(cd "$ROOT/frontend" && npm run start -- --port 4200 >"$FRONTEND_LOG" 2>&1) &
FRONTEND_PID=$!

echo "   انتظار جاهزية الـfrontend..."
for _ in $(seq 1 40); do
  if curl -sf -o /dev/null "$FRONTEND_URL/"; then
    echo "✔ الـfrontend جاهز"
    break
  fi
  sleep 3
  if ! kill -0 "$FRONTEND_PID" 2>/dev/null; then
    fail "الـfrontend توقف أثناء الإقلاع. راجع: $FRONTEND_LOG"
  fi
done

echo ""
echo "=================================================="
echo "  التطبيق يعمل محليًا:"
echo "  Frontend :  $FRONTEND_URL"
echo "  عروض الباقات (عام):  $FRONTEND_URL/preparation-packages"
echo "  Backend  :  $BACKEND_URL  (مثال: $BACKEND_URL/api/v1/preparation-packages/offers)"
echo "  Logs     :  $BACKEND_LOG  +  $FRONTEND_LOG"
echo "  للإيقاف :  اضغط Ctrl + C"
echo "=================================================="
wait