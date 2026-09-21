# Employer Screen Contracts

```yaml
document_id: NPS-DES-SCREEN-CONTRACTS-EMPLOYER
status: authority-gap-family
owner: frontend-design-governance
updated: 2026-09-21
```

## Family Authority

Routes and role policy exist, but no dedicated employer approval packet exists. Candidate/request business fields must come from future backend/API and product authority, not route names or nurse profile assumptions.

## EMP-001 Employer Home

| Field | Contract |
|---|---|
| Identity | `/employer`; Employer role. |
| Gap | Employer landing/profile purpose, data, metrics, and navigation are not approved. |
| Status | `AUTHORITY_GAP`. |

## EMP-002 Candidate Search

| Field | Contract |
|---|---|
| Identity | `/employer/candidates`; Employer role. |
| Known | Candidate search route exists. |
| Gap | Search fields, filters, visible candidate-safe fields, sorting, pagination, and privacy boundaries need dedicated authority. |
| Status | `AUTHORITY_GAP`. |

## EMP-003 Candidate Results

| Field | Contract |
|---|---|
| Known | Candidate results may be represented only after safe DTO fields are approved. |
| Forbidden | Email, license, CV, private nurse fields, raw user IDs unless backend/product authority explicitly permits. |
| Status | `AUTHORITY_GAP`. |

## EMP-004 Candidate Empty Filtered States

| Field | Contract |
|---|---|
| Gap | Empty/no-results copy and reset behavior depend on unresolved search/filter contract. |
| Status | `AUTHORITY_GAP`. |

## EMP-005 Candidate Detail

| Field | Contract |
|---|---|
| Gap | No route/backend contract for candidate detail. Candidate list item DTOs do not authorize detail profile fields. |
| Status | `BACKEND_BLOCKED`. |

## EMP-006 Candidate Request

| Field | Contract |
|---|---|
| Gap | No approved candidate contact/request workflow contract. |
| Status | `BACKEND_BLOCKED`. |

## EMP-007 Employer Requests

| Field | Contract |
|---|---|
| Identity | `/employer/requests`; Employer role. |
| Gap | Request list fields, statuses, filters, actions, and ownership display are incomplete. |
| Status | `AUTHORITY_GAP`. |

## EMP-008 Employer Request Detail

| Field | Contract |
|---|---|
| Identity | `/employer/requests/:requestId`; Employer role. |
| Gap | Request detail fields, actions, not-found/ownership behavior, and candidate-safe data are incomplete. |
| Status | `AUTHORITY_GAP`. |
