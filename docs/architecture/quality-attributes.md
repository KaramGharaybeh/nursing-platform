# Architectural Quality Attributes

[Product requirements](../product/requirements.md) own required outcomes. This document records only evidenced architectural responses. It does not set numerical service levels.

| Attribute | Approved architectural response |
|---|---|
| Maintainability and testability | Inward backend dependency direction keeps Domain and Application independent of persistence and HTTP. External interfaces allow Infrastructure substitution and focused use-case testing. Feature boundaries keep frontend workflows out of shared primitives. |
| Integrity and reliability | PostgreSQL owns durable business truth. Purchase snapshots, item-based fulfillment idempotency, atomic package-attempt consumption, immutable provenance, and one report snapshot per qualifying session protect cross-boundary history. Report generation failure is isolated from exam finalization. |
| Security and privacy | Server-side trust decisions govern protected facts; the client is a presentation boundary. Package practice content is separated from protected exam content; report outputs exclude protected question/answer data. Detailed controls and policies belong to Security. |
| Scalability | The deployment design keeps application services stateless and separates durable data from cache. The architecture supports additional application instances without moving business truth into Redis. No capacity target or scaling guarantee is approved here. |
| Accessibility and localization support | The approved frontend architecture owns accessible and RTL-aware implementation boundaries. Architecture leaves those concerns in the client layer rather than assigning them to persistence or external providers. Product owns the accessibility outcome. |

No approved numerical uptime, latency, throughput, recovery-time, or cache-hit target was found in the authorities used for this migration. Those values are not inferred from architecture goals.
