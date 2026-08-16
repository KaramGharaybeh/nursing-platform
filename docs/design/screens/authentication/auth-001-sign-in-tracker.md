# AUTH-001 — Sign In Tracker

> Recommended in-project path: `docs/design/screens/authentication/auth-001-sign-in-tracker.md`

## Status Legend
- [ ] Not started
- [~] In progress
- [x] Complete
- [!] Blocked / needs review

---

## Goal
Design, verify, and fully complete **AUTH-001 — Sign In** screen for the NursingPlatform in Penpot.

---

## Scope

### In Scope
- Page 10 — Authentication Core in Penpot
- Main board: "Authentication Core — Batch A v1" (1920px wide)
- 10 required direct-child sections
- 5 responsive boards (Desktop, Tablet, Mobile, Desktop RTL, Mobile RTL)
- Sign In form design with all states
- Backend contract alignment
- WCAG 2.2 AA compliance
- RTL readiness

### Out of Scope
- Pages 00-09 modifications
- Frontend/backend source code changes
- AUTH-002+ screens
- Git commits or staging

---

## Backend Contract — Login

### Endpoint
```http
POST /api/v1/auth/login
Authorization: Anonymous
```

### Request
```json
{
  "email": "string",
  "password": "string"
}
```

### Success Response (200 OK)
```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "2026-07-22T15:30:00Z"
}
```

### Error Responses

| Scenario | Status | Title | Detail |
|----------|--------|-------|--------|
| Empty email | 400 | Validation failed | 'Email' must not be empty. |
| Empty password | 400 | Validation failed | 'Password' must not be empty. |
| Invalid credentials | 401 | Unauthorized | Invalid credentials. |
| Unexpected error | 500 | Internal server error | An unexpected error occurred. |

### Verified Backend Limitation

The implemented login handler (`LoginCommandHandler`) throws `UnauthorizedAccessException("Invalid credentials.")` for all three authentication failure cases:

1. Email not found (`user is null`)
2. User inactive (`!user.IsActive`)
3. Password verification failed (`!_passwordHasher.Verify(...)`)

All three produce the identical 401 response: `Invalid credentials.`

**Consequence for AUTH-001 design:**
- The login form must NOT display an "Account inactive" or "Account disabled" error message, because the backend never sends that information.
- The login form must NOT infer account status from the login response.
- Account Inactive is outside the directly observable AUTH-001 login outcomes unless another implemented endpoint or authoritative contract explicitly provides that state.
- The only observable 401 outcome for the login form is: `Invalid credentials.`

### Problem Details Format
```json
{
  "type": "https://httpstatuses.com/{status}",
  "title": "{title}",
  "status": {status},
  "detail": "{detail}",
  "traceId": "{traceId}",
  "errors": { }
}
```

---

## Design Requirements

### Responsive Boards
| Board | Width | Height |
|-------|-------|--------|
| Desktop | 1440px | 1024px |
| Tablet | 768px | 1024px |
| Mobile | 390px | 844px |
| Desktop RTL | 1440px | 1024px |
| Mobile RTL | 390px | 844px |

### Main Board Sections (10)
1. Overview and Requirements — Screen purpose, user context, entry points
2. Desktop Default — Desktop sign-in form layout (1440px)
3. Tablet Default — Tablet sign-in form layout (768px)
4. Mobile Default — Mobile sign-in form layout (390px)
5. RTL Variants — Arabic/RTL layout mirrors for all breakpoints
6. Field and Interaction States — Focus, hover, active, disabled, filled
7. Validation States — Client-side and server-side validation display
8. Backend-safe Outcome States — Success, invalid credentials, validation error, unexpected error
9. Loading and Submission Behavior — Submit button loading, form disabled during submission
10. Accessibility and Developer Handoff — ARIA labels, focus order, keyboard navigation, implementation notes

### Typography
- LTR: Noto Sans
- RTL: Noto Sans Arabic (approved Arabic-compatible Noto family)

### Accessibility
- WCAG 2.2 AA
- 44x44px minimum interactive target
- Visible focus states
- Screen reader compatible
- Form errors associated with fields
- No color-only communication

---

## Penpot IDs

### Page 10 — Authentication Core
- **Page ID:** `9bd8100d-da35-8029-8008-5d963d808253`

### Main Board
- **Name:** Authentication Core — Batch A v1
- **ID:** `9bd8100d-da35-8029-8008-5d9657a5d9c8`
- **Width:** 1920px

### Sections (direct children of main board)
| # | Section Name | Penpot ID |
|---|--------------|-----------|
| 1 | Section — Overview and Requirements | `9bd8100d-da35-8029-8008-5de7e275f783` |
| 2 | Section — Desktop Default | `9bd8100d-da35-8029-8008-5de7e27bca18` |
| 3 | Section — Tablet Default | `9bd8100d-da35-8029-8008-5de7e281308e` |
| 4 | Section — Mobile Default | `9bd8100d-da35-8029-8008-5de7e286013d` |
| 5 | Section — RTL Variants | `9bd8100d-da35-8029-8008-5de7e28acbbf` |
| 6 | Section — Field and Interaction States | `9bd8100d-da35-8029-8008-5de7e2901a84` |
| 7 | Section — Validation States | `9bd8100d-da35-8029-8008-5de7e294df3d` |
| 8 | Section — Backend-safe Outcome States | `9bd8100d-da35-8029-8008-5de7e299a4d9` |
| 9 | Section — Loading and Submission Behavior | `9bd8100d-da35-8029-8008-5de7e29ea1a7` |
| 10 | Section — Accessibility and Developer Handoff | `9bd8100d-da35-8029-8008-5de7e2a342a7` |

### Responsive Boards (root-level on Page 10)
| Board | Dimensions | Penpot ID |
|-------|------------|-----------|
| Desktop Default — 1440 × 1024 | 1440 × 1024 | `9bd8100d-da35-8029-8008-5de7f8b991a8` |
| Tablet Default — 768 × 1024 | 768 × 1024 | `9bd8100d-da35-8029-8008-5de7f8c0acf2` |
| Mobile Default — 390 × 844 | 390 × 844 | `9bd8100d-da35-8029-8008-5de7f8c4c944` |
| Desktop RTL — 1440 × 1024 | 1440 × 1024 | `9bd8100d-da35-8029-8008-5de7f8c9520e` |
| Mobile RTL — 390 × 844 | 390 × 844 | `9bd8100d-da35-8029-8008-5de7f8ce22d0` |

### Page 09 (read-only reference)
- **Page ID:** `5b727796-97b4-8084-8008-5d7f57f4a1bd`
- **Main Board ID:** `5b727796-97b4-8084-8008-5d7f58eeb357`
- **Auth Entry:** `9bd8100d-da35-8029-8008-5d85d9e66d38`
- **Screen Set:** `9bd8100d-da35-8029-8008-5d886506a108`
- **First Design Batch:** `9bd8100d-da35-8029-8008-5d88b2d0292c`

---

## Task Checklist

### Phase 0 — Discovery
- [x] Read all required repository docs
- [x] Discover backend auth contracts
- [x] Verify Page 10 does not exist
- [x] Capture Page 09 fingerprints
- [x] Read design-system-audit.md
- [x] Read page-09 tracker and audit

### Phase 1 — Design
- [x] Create Page 10 in Penpot
- [x] Create main board (1920px)
- [x] Create 10 sections
- [x] Create 5 responsive boards

### Phase 2 — Verification
- [~] Visual export review — exports succeeded, pending human/independent review
- [x] Structural verification — passed (section count, parent-child, responsive count)
- [x] Protected pages unchanged — verified
- [!] RTL text correct — deferred (RTL boards not yet designed)
- [!] Accessibility check — deferred (pending independent review)
- [!] Backend contract alignment — deferred (pending independent review)

### Default Board Design
- [x] Desktop Default (1440 × 1024) — designed with split layout (brand left, form right)
- [x] Tablet Default (768 × 1024) — designed with stacked layout (brand top, form below)
- [x] Mobile Default (390 × 844) — designed with full-width stacked layout

### Default Board Corrections (per reviewer feedback)
- [x] Remember me checkbox removed from all three boards
- [x] Logo placeholders replaced with finished Brand Mark (teal rounded rect with "NP")
- [x] Illustration Area placeholder removed from Desktop
- [x] "Email Placeholder" renamed to "Email Hint" (standard form UI)
- [x] "Password Placeholder" renamed to "Password Hint" (standard form UI)

### Phase 3 — Documentation
- [x] Tracker updated with Penpot IDs
- [x] Documentation updated if needed

---

## Notes for Agent
1. Do not trust completion markers alone.
2. Trust only structural inspection + visual export + business-logic review.
3. Every form state must map to a real backend scenario.
4. No invented fields, endpoints, or error messages.
5. RTL boards must use approved Arabic-compatible Noto family.
6. Protected pages 00-09 must remain unchanged.
