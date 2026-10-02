# Product Requirements

This document owns approved required outcomes and product use cases. [Business Rules](business-rules.md) owns the conditions that govern them; [Roles and Permissions](roles-and-permissions.md) owns actor capabilities.

## Functional requirements

- The product must provide account registration, sign-in, email verification, and account/password recovery capabilities for its supported users.
- Nurses must be able to maintain professional profiles describing their qualifications and experience.
- Healthcare employers must be able to search for nurse candidates and participate in a contact-request workflow.
- The product must provide nursing mock-examination preparation, including examination participation and performance information.
- The product must offer standalone paid mock-exam access and a commercially offered preparation package as distinct choices. Their access rights remain independent under the [business rules](business-rules.md).
- A preparation package must provide access to its four approved benefits: managed study materials, an independent practice question bank, one package-scoped mock-exam attempt, and one analytical report. The package must be discoverable as a sellable offer and manageable through authorized administration.
- The current v1 Preparation Package report must present numeric exam evidence, including counts and percentages, with topic-level results and package-content guidance. Qualitative performance classifications and bands are deferred beyond the current approved v1 report output. The report's evidence, guidance, and access constraints are in [Business Rules](business-rules.md).
- Package purchasers must have a user-facing place to find their material access, practice access, exam-attempt entry, report availability, and access-expiry information. This is the approved package-workspace concept; this requirement does not prescribe a screen, route, or layout.
- The product must enable authorized administrators to manage Preparation Package commercial promotions, including discounts, promotional offers, and promo codes. This capability is scoped to Preparation Packages; promotion calculations and eligibility details are not specified here.
- The product must support administration of users, examination content, reference information, and the separately authorized preparation-package content and offers.

## Non-functional requirements

- Frontend experiences must meet WCAG 2.2 AA accessibility requirements.

## Product use cases

These describe approved actor goals and outcomes. They do not prescribe test steps or API/UI mechanics.

| Actor | Goal | Approved product outcome |
|---|---|---|
| Nurse | Establish an account and prepare a professional profile | The nurse can maintain information used for examination preparation and recruitment. |
| Account holder | Recover account/password access | The account holder can use an account/password recovery capability. |
| Nurse | Prepare for a licensing examination | The nurse can take a mock exam and obtain performance information. |
| Nurse | Acquire and use a commercially offered preparation package | Fulfillment grants the package's four distinct benefits under the [package business rules](business-rules.md). |
| Employer | Find a suitable nurse candidate | The employer can search candidate information and request recruitment contact without receiving contact information before nurse approval. |
| Nurse | Respond to a recruitment contact request | The nurse can approve or reject a received request. |
| Administrator | Maintain product content and offers | Authorized administration can manage examination and preparation-package content and sellable offers. |
| Authorized Preparation Package promotion administrator | Manage commercial promotions for Preparation Packages | Authorized administration can manage discounts, promotional offers, and promo codes for Preparation Packages. |
