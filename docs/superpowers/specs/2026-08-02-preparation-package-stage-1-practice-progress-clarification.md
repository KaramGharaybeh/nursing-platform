# Preparation Package Stage 1 Practice Progress Clarification

## Status

Approved-scope clarification draft for Stage 1 runtime practice-progress semantics.

This document clarifies only the runtime semantics needed to implement basic package practice progress after the approved Stage 1 catalog and authoring work. It does not authorize implementation, an implementation plan, source-code changes, test changes, database migrations, staging, committing, pushing, frontend/design work, or later-stage work.

## Purpose

The approved Stage 1 specification requires basic practice progress that distinguishes unanswered practice items from answered practice items and distinguishes answered-correct from answered-incorrect. The approved Stage 1 specification also defers concrete persistence and counters. This clarification resolves the minimum runtime decisions required before an implementation plan can safely define persistence, Application behavior, and nurse-owned WebApi endpoints.

## Scope

Practice progress is for package practice only. It is not official exam score, does not affect official package exam scoring, and does not affect package-attempt consumption. Practice progress is not employer-visible in v1 and is not Stage 4 analytical-report classification evidence.

Practice progress applies to the purchased package's exact practice collection version. It must stay isolated from official exam sessions, official exam questions, official exam answer options, and exam snapshots.

## Ownership and Isolation

Practice progress belongs to one nurse profile and one package purchase entitlement.

Progress is isolated per entitlement. If the same nurse later buys the same package again, or buys a different package that references the same or different practice collection version, that purchase receives independent progress. Progress from one package purchase entitlement must not satisfy, overwrite, or merge with progress from another entitlement.

## Authorization

Practice answer write actions require:

- the current authenticated user to own the package purchase entitlement through their nurse profile;
- an active package purchase entitlement access window; and
- an available `PracticeAccess` benefit right for that entitlement at the write timestamp.

Read access to historical practice progress after package expiry is allowed for the owning nurse. No practice write, answer submission, re-answer, retry, or retraining action is allowed after expiry.

Practice actions never consume, mutate, or depend on the package exam-attempt benefit right. Practice actions must not start or resume exam sessions.

## State Model

The v1 practice-progress item states are:

- `Unanswered`
- `AnsweredCorrect`
- `AnsweredIncorrect`

`Unanswered` may be derived from the absence of a stored progress row for an entitlement/practice item pair. Stored progress rows represent answered practice items only unless a future approved design identifies a stronger reason to persist explicit unanswered rows.

## Identity and Keying

Practice progress is keyed by:

- nurse profile id;
- package purchase entitlement id;
- practice collection version id; and
- practice item id.

Practice progress must not be keyed by exam session. It must not be keyed by exam question. It must not reference official exam question ids, official exam answer option ids, official exam session ids, or official exam snapshots.

The package purchase entitlement's `PracticeCollectionVersionId` is the authoritative package-practice scope. A submitted practice item must belong to that exact practice collection version.

## Answer Behavior

Submitting a practice answer records the selected practice answer option for one practice item.

Correctness is evaluated only against the selected `PracticeAnswerOption.IsCorrect` value from the independent practice answer option model. Official exam answer options, official exam answer keys, official exam rationales, and official exam snapshots are never used to evaluate practice progress.

Re-answering the same practice item is allowed while package practice access is active. Re-answering overwrites the latest selected practice answer option, latest correctness state, and latest answered timestamp for that entitlement/practice item pair.

No attempt history is stored in v1. A future retry-history model is explicitly deferred.

The latest answered timestamp must be stored for answered rows.

## Counters

V1 practice-progress counters are derived, not stored. Derived counters include:

- total practice items in the purchased practice collection version;
- answered count;
- unanswered count;
- correct count; and
- incorrect count.

No aggregate practice-progress counter table is introduced in v1.

## Persistence Expectation

A new persisted entity/table is required to store answered practice progress.

Relationships should be restrictive where appropriate, consistent with the Preparation Package persistence style. Persistence must protect historical purchasers by retaining progress independently of later catalog retirement or ineligibility.

The table must enforce a uniqueness constraint preventing duplicate progress rows for the same package purchase entitlement and practice item. The implementation plan may include additional indexes for nurse-owned reads, entitlement reads, and practice collection version reads, provided they do not alter the ownership or isolation rules above.

## Endpoint Expectations

A nurse-owned read endpoint is required for package practice progress summary and item states.

A nurse-owned submit-answer endpoint is required for one practice item.

Exact route shapes, request DTOs, response DTOs, status codes, and Problem Details codes may be finalized in the implementation plan. All routes must remain under `/api/v1/me/nurse-profile/preparation-packages/...` and must be owned by the current nurse profile.

No employer practice-progress routes are allowed in v1. No admin practice-progress routes are allowed in v1 unless separately approved.

## Security and Privacy

Practice progress responses must not expose official exam question text, official exam answer option text, official correct option identifiers, official exam rationales, official answer keys, or official exam snapshots.

Practice content may expose its own authored prompt, answer options, and immediate-feedback content only according to the existing independent practice content model and the approved practice runtime endpoint contract.

Practice progress responses must not expose internal package benefit-right ids, internal authorization state, raw tokens, secrets, or EF/domain navigation objects.

## Stage 4 Compatibility

Package analytical reports must not use practice progress as analytical classification evidence.

Practice may remain a deterministic guidance reference only through the purchased practice collection version and practice-item-to-reporting-topic mappings. Stage 4 report evidence remains based on finalized package exam-session snapshots and compatible reporting-profile assignments.

## Explicitly Deferred

The following are deferred and must not be implemented as part of basic v1 practice progress unless separately approved:

- retry history;
- spaced repetition or retraining algorithms;
- adaptive practice;
- workspace or dashboard aggregation;
- employer visibility;
- practice progress as analytical-report evidence;
- offline sync;
- progress export; and
- cross-package progress merging.

## Non-Authorization

This clarification is not an implementation plan. It does not authorize source-code changes, tests, migrations, endpoint additions, staging, committing, pushing, or proceeding to another Stage 1 runtime item.
