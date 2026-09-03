# Frontend Implementation Ledger

This ledger is the detailed execution record for frontend Goal → Milestone → Task → Subtask work.

It is a governance and tracking document only. It does not authorize implementation by itself.

## 1. Authority

Authoritative frontend rules remain in:

- `docs/frontend/frontend-project-rules.md`
- `docs/frontend/frontend-architecture.md`
- approved design specifications under `docs/frontend/design/specs/`
- approved backend/OpenAPI contracts under `docs/frontend/design/integration/openapi/`

This ledger records execution status, evidence, and handoff details after an implementation plan is explicitly approved.

## 2. Relationship To `PROGRESS.md`

`PROGRESS.md` remains the high-level canonical session and handoff memory for the repository.

Use `PROGRESS.md` for:

- current Goal;
- current Milestone;
- active Task;
- latest verified checkpoint;
- blockers;
- next approved gate;
- cross-cutting decisions that affect the next agent;
- pointer to this detailed ledger.

Use this ledger for detailed frontend implementation history:

- Goal records;
- Milestone records;
- Task records;
- Subtask records;
- affected files/modules;
- acceptance criteria;
- verification evidence;
- design/API/source references;
- reviewer decisions;
- reopening history.

When milestone/current-state changes, update both `PROGRESS.md` and this ledger consistently.

## 3. Status Vocabulary

Use these exact statuses:

- `NOT STARTED` — recorded but not started.
- `IN PROGRESS` — currently being executed.
- `BLOCKED` — stopped pending clarification, external dependency, or approval.
- `READY FOR REVIEW` — implementation/evidence submitted for review.
- `VERIFIED` — all applicable Definition of Done checks and reviewer acceptance passed.
- `REOPENED` — previously verified scope was explicitly reopened by a later approved task.

Do not mark a Task or Subtask `VERIFIED` without evidence.

A dependent Task cannot begin, bypass, or be marked `VERIFIED` when a predecessor dependency has a failed, missing, or `BLOCKED` verification gate unless an explicit reviewer decision changes the dependency relationship.

## 4. Required Record Shape

### Goal Template

```markdown
## Goal G-### — <name>

- Status:
- Authority / approval reference:
- Product scope:
- Out-of-scope:
- Backend/OpenAPI reference:
- Penpot/design reference:
- Milestones:
- Current risk notes:
- Verification Gate:
```

### Milestone Template

```markdown
### Milestone M-### — <name>

- Status:
- Parent Goal:
- Approval reference:
- Entry criteria:
- Exit criteria:
- Tasks:
- Verification summary:
- Verification Gate:
- Reviewer decision:
```

### Task Template

```markdown
#### Task T-### — <name>

- Status:
- Parent Milestone:
- Explicit approval reference:
- Dependencies:
- Authorized affected files/modules:
- Owned files/modules:
- Out-of-scope files/modules:
- Backend/OpenAPI snapshot:
- Penpot/design reference:
- Business rules:
- Architecture/security/accessibility rules:
- Accessibility requirements:
- Responsive requirements:
- RTL requirements:
- Security/privacy requirements:
- Acceptance criteria:
- Tests:
- Subtasks:
- Verification Gate:
- Verification commands and results:
- Git status evidence:
- Decisions:
- Limitations:
- Blockers:
- Review result:
- Reviewer decision:
- Completion commit, if any:
- Reopen history:
```

### Subtask Template

```markdown
##### Subtask ST-### — <name>

- Status:
- Parent Task:
- Authorized files/modules:
- Acceptance criteria:
- Tests:
- Verification Gate:
- Verification evidence:
- Decisions:
- Limitations:
- Blockers:
- Notes:
```

## 5. Scope Protection Rules

- Do not add implementation tasks to this ledger until a Goal → Milestone → Task → Subtask implementation plan is explicitly approved.
- Do not begin the next Task from the ledger without explicit approval.
- Do not modify completed `VERIFIED` scope unless an approved task explicitly marks it `REOPENED`.
- Do not use this ledger to expand task scope during implementation.
- Do not treat a `NOT STARTED` entry as approval to code.

## 6. Reopening Rules

If a later approved Task must modify previously `VERIFIED` scope, record:

- the prior Goal/Milestone/Task/Subtask being reopened;
- why reopening is required;
- the new approval reference;
- exact affected files/modules;
- regression risks;
- required re-verification commands;
- reviewer decision returning the scope to `VERIFIED`.

No opportunistic cleanup of verified frontend scope is allowed.

## 7. Current Ledger State

No frontend implementation Goal, Milestone, Task, or Subtask entries are approved in this ledger yet.

The next authorized step is to build and approve the complete frontend implementation plan. That planning step must not scaffold Angular, create `frontend/`, install dependencies, generate an API client, modify backend source, modify Penpot, stage files, commit, or push unless a later explicit approval says otherwise.
