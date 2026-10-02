# Preparation Package Report Snapshot

Status: Accepted for the current v1 numeric report scope.

## Context and authority

The approved Stage 4 analytical-report specification distinguishes a package report from mutable aggregate exam analytics. The completed Product authority sets the current v1 report to numeric evidence and percentages, without qualitative performance bands or classification thresholds.

## Decision

One qualifying finalized package exam session can yield one immutable, nurse-owned analytical report snapshot. The first valid direct request creates it if absent; repeated or concurrent requests converge on the same report. The report uses the finalized session, its immutable package provenance, and the purchased compatible reporting-profile publication. Practice progress is not scoring evidence. Exam finalization remains independent of report generation, and failed generation leaves a recoverable path.

The approved Stage 4 v1 design does not add a report-generation failure table. Recovery detects a qualifying finalized session without its report and retries generation on direct request. The current query path follows that model and uses report-session uniqueness to converge concurrent requests.

## Rationale and consequences

A point-in-time snapshot prevents later changes to exam authoring, package content, topic names, payments, or entitlement state from changing an existing report. Separating report generation from exam finalization avoids making a reporting failure undo a completed attempt. Preserved purchased material and practice references can produce deterministic guidance without exposing protected exam content. [Product requirements](../../product/requirements.md) own required report outputs and access behavior; this record owns snapshot and boundary rationale.
