# Test Data Catalog

This document identifies verified data sources and their use in verification. It does not define Product reference values or turn a proposed fixture catalog into provisioned data. [Testing Strategy](testing-strategy.md) owns isolation principles; [Dependency Graph](dependency-graph.md) owns cross-scenario prerequisites. Never copy real personal data, passwords, tokens, or API keys into a fixture catalog or test output.

## Current sources

| Source | Verified data/actors | Contract boundary |
|---|---|---|
| `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/ReferenceDataSeeder.cs` | Startup seed for country, language, role, permission, and role-permission reference rows. Country codes include `US` and `GB`; language codes include `EN` and `AR`. | These are current seed implementation values. Consumers should resolve by stable code where the test needs a reference; hard-coded GUIDs are not Product identity. |
| `backend/src/NursingPlatform.Infrastructure/Persistence/Seed/DevelopmentExamSeeder.cs` | One Development-only published, free exam (`dev-addition-practice`) with published version 1 and 30 ordered simple-addition questions. Each question has four numeric options and one correct answer. The seeder runs after reference data and reconciles repeated Development startup to this content. | Verified by `DevelopmentExamSeederTests.cs`. This is local implementation data, not production content or a general E2E fixture contract; existing sessions can prevent destructive reseeding. |
| Backend test projects under `backend/tests/` | Test-local builders, mocks, and fixtures in individual test projects; persistence tests may need a PostgreSQL connection supplied to the test process. | Values generated inside one test are implementation data, not a shared canonical persona. |
| `frontend/e2e/fixtures.ts` and `frontend/e2e/global-setup.ts` | Two named synthetic nurse identities (`NURSE_A`, `NURSE_LC`) provisioned via real API flows for the current AUTH Playwright suite; an ephemeral nurse helper uses a per-run tag. | These identities are scoped to disposable local AUTH verification. Credential literals remain in the fixture implementation and are deliberately not reproduced here. |
| `frontend/e2e/run-auth-e2e.sh` and `frontend/e2e/test-env-guard.mjs` | Disposable local PostgreSQL and mail-capture prerequisites with explicit opt-in and environment guard. | The guard protects this particular E2E runner; it is not evidence of a provisioned full-system test environment. |

## Data handling

- Use synthetic identities and question/content data for tests. A fixture is not an approved Product persona or real clinical content.
- Preserve independence between test runs. Current AUTH Playwright setup provisions through the API on a disposable database; other scenarios need their own verified setup rather than assuming the historical catalog is provisioned.
- The old Step 3 catalog at this path specified 6 proposed identities and 32 Data IDs, including 16 planned reusable resources, and explicitly stated that nothing in that catalog was provisioned. Those proposed exact slugs, values, and resource definitions remain recoverable from Git history; they are not current shared fixture guarantees.
- Where a test requires email delivery, use the verified local mail-capture arrangement for the AUTH runner. Do not use a real mailbox or disclose captured tokens in documentation.

The existence and contents of any additional shared test dataset must be established from its actual provisioning code before being added here.
