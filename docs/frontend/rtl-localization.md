# RTL and Localization

This document owns frontend directionality and localization implementation conventions. It does not approve additional product languages or claim complete translation coverage. The approved frontend foundation and active design source support English and Arabic typography and RTL-aware layout. Full Arabic localization and full RTL screen coverage were deferred in the earlier frontend foundation; current code now establishes a partial English/Arabic mechanism, not proof that the full target has been accepted or completed.

## Current implementation

`frontend/src/app/core/locale/locale-direction.service.ts` manages locale/direction state and applies document direction. `frontend/src/app/core/i18n/localization.service.ts` supplies translated UI strings from the current translation source. Existing code demonstrates English and Arabic handling; it does not establish completeness across every screen, error, API value, or persisted content.

Frontend styles use logical start/end properties and locale-aware alignment rather than hard-coded left/right assumptions. Directional icons are mirrored selectively; brand and status icons are not mirrored merely because direction changes. Mixed-direction numbers, punctuation, and dates require readable presentation. Arabic text must remain native/editable, and arbitrary letter spacing or forced shrinking is prohibited by the active design foundation.

The active visual source owns typography and layout tokens; [Design System](design-system.md) maps it to Angular implementation. [Screen Contracts](screen-contracts/README.md) own screen-specific RTL states and approved presentation. Product owns any approved language-service requirement; an Angular capability alone does not establish one.
