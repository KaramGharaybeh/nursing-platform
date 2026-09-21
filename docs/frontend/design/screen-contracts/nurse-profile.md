# Nurse Profile Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-NURSE
status: deep-extracted-family-contract
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Primary authority: `nurse-screen-approval-packet.md`, `system-design-contract.md`, canonical route/permission policy, current nurse-profile backend/OpenAPI contracts, and verified frontend implementation evidence. All screens are Nurse-role scoped unless noted.

## NUR-ENTRY Nurse Entry

| Field | Contract |
|---|---|
| Identity | `NUR-ENTRY`; route `NURSE_ENTRY`; `/nurse`; role Nurse. |
| Gap | Final landing/redirect behavior is route/task-owned and must not invent a dashboard. Current authority allows Nurse profile as the safe destination but does not define a standalone screen. |
| Status | `AUTHORITY_GAP`. |

## NUR-001 Profile Overview

| Field | Contract |
|---|---|
| Identity | `NUR-001`; route `NURSE_PROFILE_OVERVIEW`; `/nurse/profile`; role Nurse; routable. |
| Purpose | View current nurse professional profile summary and section entry points. Non-goals: profile score, public preview/share, recruiter analytics, fake identity. |
| Data | `headline`, `professionalSummary`, `licenseNumber`, `licenseCountry`, `currentCountry`, `yearsOfExperience`, `isAvailableForRecruitment`, section counts/previews, CV metadata if supplied. Missing profile uses first-time/empty state; optional sparse fields show neutral absence copy. |
| Navigation | Entry from shell Profile/Nurse entry; exits to personal information, experience, education, certificates, skills, languages, CV, contact requests where implemented. |
| Actions | Section management links only; no profile completion score or share action. |
| States | Loading, loaded, sparse, first-time 404 empty/create prompt, retryable error. |
| Status | `CONTRACT_READY`. |

## NUR-002 Profile Summary State

| Field | Contract |
|---|---|
| Identity | `NUR-002`; same route `/nurse/profile`; profile summary/first-time state. |
| Purpose | Represent no-profile or sparse-profile summary without creating a separate route. |
| Data/states | No-profile state must invite creating personal information; sparse state must not imply incompleteness percentage. |
| Status | `CONTRACT_READY`. |

## NUR-003 Personal Information

| Field | Contract |
|---|---|
| Identity | `NUR-003`; `/nurse/profile/personal-information`; role Nurse. |
| Purpose | Create/update base professional profile. |
| Form fields | `headline` text, optional/required per backend, professional headline; `professionalSummary` textarea; `licenseNumber` text; `licenseCountry` select from countries; `currentCountry` select from countries; `yearsOfExperience` numeric, non-negative backend range authoritative; `isAvailableForRecruitment` checkbox boolean. |
| Actions | `Save`: create/update profile; `Cancel`/Back: overview. |
| States | 404 create mode, edit existing, validation, saving, saved, 409 country/reference conflict, retryable error. |
| Forbidden | Employment type, privacy tiers, unsupported verification, fake credentials. |
| Status | `CONTRACT_READY`. |

## NUR-004 Experience List

| Field | Contract |
|---|---|
| Identity | `NUR-004`; `/nurse/profile/experience`; role Nurse. |
| List item order/data | `jobTitle`, `facilityName`, `country`, `startDate`, `endDate`, `isCurrent`, `description`; order from backend/implementation authority. Dates formatted locale-aware. |
| Actions | Add, edit, delete. Delete requires inline confirmation in `NUR-005` flow. |
| States | Loading, empty, list, saving refresh, stale/deleted notice, error. |
| Status | `CONTRACT_READY`. |

## NUR-005 Experience Form State

| Field | Contract |
|---|---|
| Form fields | `jobTitle` text required; `facilityName` text required; `country` select required; `startDate` date required; `endDate` date optional; `isCurrent` checkbox boolean, clears/disables `endDate` when true; `description` textarea optional. Date rule: if end exists, end >= start. |
| Actions | Save creates/updates; Cancel returns to list; Delete item confirmation uses `Keep` and `Delete`. |
| Status | `CONTRACT_READY`. |

## NUR-006 Education List

| Field | Contract |
|---|---|
| Data | `institution`, `degree`, `fieldOfStudy`, `country`, optional `startDate`, optional `endDate`. |
| Actions/states | Add/edit/delete with empty/list/form/validation/delete/error states. |
| Status | `CONTRACT_READY`. |

## NUR-007 Education Form State

| Field | Contract |
|---|---|
| Form fields | `institution` text required; `degree` text required; `fieldOfStudy` text; `country` select; `startDate` optional date; `endDate` optional date; if both dates exist, end >= start. |
| Status | `CONTRACT_READY`. |

## NUR-008 Certificates List

| Field | Contract |
|---|---|
| Data | `name`, `issuingOrganization`, `issueDate`, optional `expirationDate`, optional `credentialId`, optional `credentialUrl`. Credential URL displays as external link only when valid. |
| Actions/states | Add/edit/delete; empty/list/form/url validation/delete/error. No verified/official badge. |
| Status | `CONTRACT_READY`. |

## NUR-009 Certificate Form State

| Field | Contract |
|---|---|
| Form fields | `name` text required; `issuingOrganization` text required; `issueDate` date; `expirationDate` optional date; `credentialId` optional text; `credentialUrl` optional absolute `http`/`https` URL. |
| Status | `CONTRACT_READY`. |

## NUR-010 Skills

| Field | Contract |
|---|---|
| Data/form | Free-text skill names as removable chips. Input trims/collapses whitespace; duplicates invalid case-insensitively; max 50 skills; max 100 characters per skill. |
| Actions | Add/remove locally, Save full replace, Cancel/back to overview. No taxonomy/autocomplete. |
| Status | `CONTRACT_READY`. |

## NUR-011 Languages

| Field | Contract |
|---|---|
| Data/form | Rows with `language` select from backend languages and `proficiency` enum `Beginner`, `Intermediate`, `Advanced`, `Fluent`, `Native`; max 20; duplicate language invalid. |
| Actions/states | Add/remove rows, Save full replace, validation, saving, error. |
| Status | `CONTRACT_READY`. |

## NUR-012 CV

| Field | Contract |
|---|---|
| Identity | `/nurse/profile/cv`; route exists. |
| Data/form | Current CV metadata: file name, size, uploaded date when backend supplies. Upload accepts `.pdf`, `.doc`, `.docx`, <=5MB. Selected file requires explicit Upload/Replace. Delete requires confirmation. |
| Gap | System contract records current route/component gap may remain in implementation reality; no preview/download unless backend re-authorizes. |
| Status | `PARTIAL`. |

## NUR-013 Profile Preview Share

| Field | Contract |
|---|---|
| Gap | Intentional deferred concept. No public preview/share route or product behavior. |
| Status | `DEFERRED`. |

## NUR-CONTACT Contact Requests

| Field | Contract |
|---|---|
| Identity | `NUR-CONTACT`; route `NURSE_CONTACT_REQUESTS`; `/nurse/contact-requests`; role Nurse. |
| Purpose | Review employer contact requests and respond to pending requests. |
| List/table | Identity: requester/employer safe DTO facts; metadata: status, created date. Actions: approve pending request, reject pending request. Filters: status filter where contract supports; pagination where backend supplies. Empty and filtered-empty distinct. |
| Confirmation | Reject uses inline irreversible confirmation: `Keep` / `Reject`; approve has no V1 confirmation but duplicate activation is blocked. |
| Privacy | No raw request IDs, no employer private internals, no permission keys. |
| Status | `CONTRACT_READY`. |
