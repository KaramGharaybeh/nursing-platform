# Administration Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-ADMIN
status: mixed-ready-and-gap-family
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Routes and permission policies exist for admin routes. Only user list/detail currently have enough screen/API evidence for contract-ready treatment. Future admin screens require owning screen packets or backend/API extraction before Stitch/implementation.

## ADM-ENTRY Admin Entry

| Field | Contract |
|---|---|
| Identity | `/admin`; role Admin. |
| Gap | Admin entry must not become fake metrics dashboard. Final workspace/landing behavior remains unresolved. |
| Status | `AUTHORITY_GAP`. |

## ADM-002 Admin Users

| Field | Contract |
|---|---|
| Identity | `/admin/users`; role Admin + `Users.View`. |
| Purpose | Find users. |
| Table/list | User identity; email/username/name; roles; active/status facts where DTO supplies. Column/order must prioritize identity, email/username, roles/status, actions. |
| Search/pagination | Implemented search/pagination authority; no unapproved filters/bulk actions. |
| Row actions | View detail -> `ADM-003`. |
| Privacy | No passwordHash, secrets, tokens; sensitive-field tests inspect raw JSON. |
| Responsive | Desktop table allowed; mobile cards preserve identity, roles/status, action. |
| Status | `CONTRACT_READY`. |

## ADM-003 Admin User Detail

| Field | Contract |
|---|---|
| Identity | `/admin/users/:userId`; role Admin + `Users.View`. |
| Purpose | View user detail and manage currently implemented role/detail actions only. |
| Data | Safe user detail DTO fields: email/username/name/roles/permissions/profile state where authorized; no passwordHash/tokens. |
| Forms/actions | Role update form only where implemented and authorized; role value from approved role list/backend; save uses admin role endpoint. |
| States | Loading, ready, not found, validation, saving, saved, forbidden/error. |
| Status | `CONTRACT_READY`. |

## ADM-005 Reference Data

| Field | Contract |
|---|---|
| Identity | `/admin/reference-data`; role Admin + `Exams.View`. |
| Gap | Screen fields, tables, actions, and edit capabilities are not extracted/approved. |
| Status | `AUTHORITY_GAP`. |

## ADM-006 Admin Exams

| Field | Contract |
|---|---|
| Identity | `/admin/exams`; role Admin + `Exams.View`. |
| Gap | Admin exam list columns/actions/statuses are incomplete. |
| Status | `AUTHORITY_GAP`. |

## ADM-007 Admin Exam Detail

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId`; role Admin + `Exams.View`. |
| Gap | Detail fields/actions are incomplete. |
| Status | `AUTHORITY_GAP`. |

## ADM-008 Admin Exam Versions

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId/versions`; role Admin + `Exams.View`. |
| Gap | Version list/status/actions are incomplete. |
| Status | `AUTHORITY_GAP`. |

## ADM-QUESTIONS Admin Exam Questions

| Field | Contract |
|---|---|
| Identity | `/admin/exams/:examId/questions`; role Admin + `Questions.View`. |
| Gap | Question list/content boundaries/actions are incomplete; protected exam content rules must be extracted before design. |
| Status | `AUTHORITY_GAP`. |

## ADM-PAY-PRODUCTS Admin Payment Products

| Field | Contract |
|---|---|
| Identity | `/admin/payment-products`; role Admin + `Exams.View`. |
| Gap | Permission naming caveat and screen actions/columns need resolution before implementation assumptions. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-TOPICS Reporting Topics

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/topics`; role Admin + `ReportingTopics.Manage`. |
| Gap | Admin topic fields/actions/statuses need owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-PROFILES Reporting Profiles

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/profiles`; role Admin + `ReportingProfiles.Manage`. |
| Gap | Reporting-profile list/detail/publication workflow needs owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-MATERIALS Study Materials

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/materials`; role Admin + `StudyMaterials.Manage`. |
| Gap | Material authoring/version fields and actions need owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-PRACTICE Practice Collections

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/practice-collections`; role Admin + `PracticeCollections.Manage`. |
| Gap | Practice collection fields/actions need owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-DEFINITIONS Package Definitions

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/definitions`; role Admin + `PreparationPackages.View`. |
| Gap | Definition/version composition fields/actions need owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-PP-OFFERS Package Offers

| Field | Contract |
|---|---|
| Identity | `/admin/preparation-packages/offers`; role Admin + `PreparationPackageOffers.Manage`. |
| Gap | Offer lifecycle fields/actions need owning extraction. |
| Status | `AUTHORITY_GAP`. |

## ADM-DASHBOARD

| Field | Contract |
|---|---|
| Gap | No stable dashboard/metrics/statistics data contract. Do not infer metrics from database counts or design expectations. |
| Status | `BACKEND_BLOCKED`. |

## ADM-004 Roles And Permissions

| Field | Contract |
|---|---|
| Gap | No stable role/permission management backend contract. Permission constants and `/me` roles do not authorize management UI. |
| Status | `BACKEND_BLOCKED`. |

## ADM-009 Admin Payment Orders

| Field | Contract |
|---|---|
| Gap | No admin payment-order management contract. Nurse-owned payment order routes do not authorize admin order UI. |
| Status | `BACKEND_BLOCKED`. |

## ADM-010 Admin Recruitment

| Field | Contract |
|---|---|
| Gap | No admin recruitment-management contract. Employer contact-request/candidate routes do not authorize admin recruitment UI. |
| Status | `BACKEND_BLOCKED`. |
