# Verification Dependency Graph

This graph owns prerequisites for cross-boundary verification. Arrows express evidence-supported state dependencies, not a global test run order or a Product roadmap. [Test Data Catalog](test-data-catalog.md) identifies verified data sources. Expected behavior remains with [Product](../product/requirements.md), technical owners, and [E2E Scenarios](end-to-end-scenarios.md).

```text
Reference data and isolated environment
  ├─> actor account / verification / sign-in
  │     ├─> nurse profile ─> nurse-owned exam and package journeys
  │     ├─> employer profile ─> contact request ─> nurse response
  │     └─> authorized administrator ─> published exam/content
  │                                  └─> published package version ─> offer
  ├─> published exam ─> exam session ─> finalized result
  └─> offer ─> purchase / successful fulfillment ─> entitlement
                                                  ├─> materials / practice
                                                  └─> package exam session
                                                        └─> finalized qualifying session
                                                              └─> package report
```

| Dependency | Evidence / reason for ordering |
|---|---|
| Reference seed before consumers | `DatabaseInitializer` invokes `ReferenceDataSeeder` during startup; tests consuming those references need resulting rows or an isolated equivalent. |
| Account/role before protected journeys | [Roles](../product/roles-and-permissions.md) define actor capabilities; [Security](../security/authentication-authorization.md) owns protected enforcement. Anonymous and forbidden checks are separate negative paths. |
| Published exam/content before package publication | [Package composition rules](../product/business-rules.md) bind exact published versions, including a compatible reporting profile, before a package version can be sold. |
| Offer before purchase; fulfillment before entitlement | [Package purchase rules](../product/business-rules.md) and [Architecture runtime flows](../architecture/runtime-flows.md) establish the sequence. |
| Entitlement before package benefit use; finalized qualifying session before report | [Package access/report rules](../product/business-rules.md) establish these prerequisites. |
| Employer request before nurse response/contact release | [Recruitment contact rule](../product/business-rules.md) makes approval the gate for contact access. |

The current AUTH Playwright runner has additional local prerequisites in `frontend/e2e/run-auth-e2e.sh` and `frontend/e2e/global-setup.ts`. These belong to that implementation path, not every future E2E scenario. The former Step 4 graph at this path modeled a wider scenario sequence and proposed IDs; it remains available in Git history as historical planning evidence.
