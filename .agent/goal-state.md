# Goal State — EXAMS-STITCH-END-TO-END-INTEGRATION-2026-09-27

## Goal Metadata
- **Goal ID:** EXAMS-STITCH-END-TO-END-INTEGRATION-2026-09-27
- **Status:** Active — model authority granted (HUMAN_EXPLICIT); EXM-001 resumes after the policy commit
- **Branch:** `feat/2026-09-27-stitch-ui-refactor` (verified)
- **Starting checkpoint HEAD:** `1debefc66b1fe126ff5ba8166110e98ff4e34ab4` (local commit, no push)
- **Pre-checkpoint baseline:** `c139fe39e8674590cc466863ca8293ee5cd2a31b` (verified)
- **Prior goals:** `STITCH-UI-REFACTOR-2026-09-27` SUPERSEDED; earlier `EXAMS-END-TO-END` draft state replaced by this human-named goal. Prior closures preserved: Batch1 `e47e2b2`, Batch2 `6d4aa6c`, corrections `7b78919`/`c139fe3`, localization/shared/password checkpoint `1debefc`.
- **Authorization:** Human decision 2026-09-27 — model authority update + EXM-001 start + exact-scope local commit authorized; NO PUSH; no Playwright MCP.

## Primary Model Authority (HUMAN_EXPLICIT)
- **Primary execution model for this goal:** `openai/gpt-6-sol`
- **Authorization:** HUMAN_EXPLICIT (intentional human authorization, not silent fallback, not accidental substitution).
- **Reason:** Human selected GPT-6 Sol as the primary model for the Exams implementation and integration goal.
- **Applies to:** EXM-001, EXM-002, EXM-004, EXM-005, EXM-006, EXM-007, EXM-008, EXM-009, EXM-010, PP-EXAM-START, PP-007, ADM-006, ADM-008, ADM-QUESTIONS — unless the goal is later explicitly changed by the human.
- **Scope limit:** This goal only. Unrelated goals retain existing defaults: primary `opencode/muse-spark-1.3-contributor-free` per `docs/development/model-orchestration.md` Active model routes, `docs/development/opencode-agent-runtime.md` role table, and `.opencode/agents/dev-orchestrator.md` frontmatter `model:` + line-109 free-model rule.
- **Suspended for this goal only:** the dev-orchestrator profile rule requiring the free model for ordinary execution. Suspension is explicit and recorded here; it is not a silent OpenAI substitution. Do not silently route this goal back to the free model.
- **Verifier/expert:** UNCHANGED. Verifier remains mandatory read-only review at coherent batch boundaries; expert remains advisory escalation only. No verifier/expert model-policy change in this update.

## Override Mechanism (no new global policy)
- The existing canonical architecture already provides the goal-level exception path: `docs/development/model-orchestration.md` requires explicit user authorization for staging/commits/pushes and permits an explicitly authorized GOAL to allow bounded exact-scope LOCAL staging/committing. This HUMAN_EXPLICIT model selection rides that existing path.
- No new reusable override mechanism invented; no global default changed. Smallest coherent change is this file only. Tracked governance docs (`model-orchestration.md`, `opencode-agent-runtime.md`, `.opencode/agents/*`) and `opencode.jsonc` (sensitive dirty state) are intentionally untouched and unstaged by this update.

## Goal Constraints (unchanged)
- No Playwright MCP anywhere in this goal. No push. Exact staging only (`git add` explicit paths; never `.`/`-A`; no reset/restore/clean/stash/amend/rebase).
- No superseded, rejected, or AUTHORITY_UNCLEAR Stitch artifacts. No invented EXM-003 (DEFERRED inline state) or ADM-007 designs.
- No EXM/production code modified in this policy turn (governance/config update kept separate from feature implementation).

## Completed — Prior Checkpoint `1debefc`
- 158 files: system-wide EN/AR localization, shared controls/theme (`_tokens.scss`, new `_controls.scss`, persistent-label form controls, localized loading/retry), password visibility toggle, related specs, `STITCH_UI_REFACTOR_REPORT.md` follow-up section. Safeguards reused per instruction; `git diff --cached --check` clean; staged empty after commit; no push.

## Completed — Model Authority (this turn, pre-commit)
- This file updated as the goal-scoped override record. No other files changed. No production code touched.

## Blockers / Open Questions
- None for model authority. EXM-001 resumes after the policy commit from accepted artifact `88c5c6b9…` (`EXM-001`, `/exams`) using ledger/contract/registry/DESIGN.md authority already in repo (note: `T-FE-067/068/069/071/072/074/073` task records read VERIFIED with stale matrix `BLOCKED/NOT STARTED` — do not duplicate VERIFIED scope without explicit reopen; visual/Stitch alignment vs functional scope to be confirmed per screen from contracts before any edit; stop rather than invent).

## Next Action
- Exact-stage only `.agent/goal-state.md`, inspect `git diff --cached --name-status` + full cached diff + `git diff --cached --check`, create one local commit `chore(orchestration): authorize GPT-6 Sol for Exams goal`, NO PUSH. Then resume EXM-001 reconnaissance read-only (accepted artifact + contract + current implementation comparison; no prod edits, no Playwright, no push).
