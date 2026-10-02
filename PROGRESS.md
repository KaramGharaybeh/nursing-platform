# Project Current State

> Compact handoff; durable task evidence lives in the owning execution ledgers and on-demand history in `PROGRESS_HISTORY.md`, not here.

## Current Goal — EXAMS-MISSING-NURSE-PROFILE-CARD-2026-09-29

- **Complete — READY_FOR_HUMAN_VISUAL_CHECK** on `feat/2026-09-27-stitch-ui-refactor` @ `cbfe1eb89b15e7d1ab86c1cbc553df9d32077838`. The bounded 2026-09-29 goal recorded independent read-only review PASS with no required corrections. `/exams` shows a localized informational status card and canonical `/nurse/profile` CTA on missing NurseProfile HTTP 404 only; no catalog request, backend/auth/DB change, or redirect. Profile-present catalog and unexpected-error/retry paths were preserved. Durable behavior is in `docs/frontend/screen-contracts/exams.md`; acceptance/evidence status is in `docs/frontend/execution/frontend-implementation-ledger.md`.
- Primary evidence from preserved checkpoint (not rerun on resume): focused 24/24, neighbors 118/118, lint/styles PASS, build PASS (nonfatal `exams-list.scss` 5.15kB/4kB budget warning), diff-check PASS. Verifier independently inspected the unchanged five-file diff, status/HEAD, clean diff-check and empty staged set. No Playwright/real-browser verification; human visual check is outstanding, not approved. No stage, commit, or push.

## Protected Worktree / Boundaries

- The prior Development-only 30-addition-question seed goal recorded a corrected build and independent rereview PASS. Its durable Development startup facts and test ownership are in `docs/operations/deployment-runbook.md` and `docs/testing/test-data-catalog.md`; its implementation remains uncommitted product work. The separate exam-session interaction work is also uncommitted and is not included in this preservation phase.
- Exam flag migration files `20260928070124_AddExamSessionQuestionFlag.cs` and `.Designer.cs` are present locally but untracked. Database application status is unverified from repository inspection.
- Preserve all unrelated dirty tracked/untracked files, especially shared `translations.ts` `session.*` hunks, backend exam-session work, design/governance docs, `.gitignore`, `frontend/angular.json`, `skills/`, `.playwright-mcp/`, `.superpowers/`, `0`, `Playwright MCP Full Review/`, `logs/`, and local scripts. Never broadly stage or push.
- External `ATRIA_API_KEY`, `PENPOT_MCP_USER_TOKEN`, `STITCH_API_KEY` rotation remains human-owned and blocks only affected provider/MCP use. No credentials in repository/output. Separate COM-004/005/006 and production-only/security branches are outside this goal.

## Next Authorized Action

- Human visual check of the `/exams` missing-profile card in English, Arabic, and RTL remains pending. No further implementation is authorized by the completed bounded goal. Consult the EXM-001 screen contract, frontend implementation ledger, and current product source/tests before making a broader Exams completion or commit claim.
