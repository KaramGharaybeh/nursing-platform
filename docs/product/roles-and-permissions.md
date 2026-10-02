# Roles and Permissions

This document defines approved product actors and the business meaning of their capabilities. Authentication mechanisms, server policies, frontend guards, and endpoint permissions belong to their technical owners.

## Actors

- **Nurse:** a person using the product for professional profile management, examination preparation, package benefits, and responses to recruitment contact requests.
- **Employer:** a healthcare organization user seeking qualified nurse candidates and requesting recruitment contact.
- **Administrator:** an authorized user maintaining product users, examination and reference content, and separately governed preparation-package content, offers, and commercial promotions.
- **Visitor:** a person who has not established an authenticated product session; only specifically public product capabilities apply.

These are product actor categories. They do not define token claims, seeded role names, or a complete technical permission registry.

## Approved capability boundaries

| Actor | Product capability | Boundary |
|---|---|---|
| Visitor | Discover publicly offered preparation packages | Public catalog information excludes protected examination and internal authorization content. |
| Nurse | Maintain the nurse's own professional profile and use the nurse's own examination and purchased-package rights | Another nurse's entitlement, practice progress, or package report is not conferred by this role. |
| Nurse | Respond to a recruitment contact request received for the nurse | Contact access for an employer follows nurse approval. |
| Employer | Discover eligible nurse candidates and request recruitment contact | Candidate discovery does not grant pre-approval contact information or package purchase/report access. |
| Administrator | Manage authorized product, examination, and reference content | Administration does not imply access to a nurse's package report in v1. |
| Authorized package content administrator | Manage package composition, reporting topics/profiles, materials, and practice collections | These capabilities require dedicated package/content permissions; examination-content permissions alone do not confer them. |
| Authorized package offer administrator | Configure and control package offers | Offer administration chooses an already-published package version and commercial terms; it does not alter that version's content. |
| Authorized Preparation Package promotion administrator | Manage discounts, promotional offers, and promo codes for Preparation Packages | This capability does not authorize platform-wide promotion administration. |

The package administration rows describe capability scopes, not additional named account roles. Concrete permission identifiers and enforcement contracts are owned by the security, backend, and API documentation.
