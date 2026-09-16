import { Component, inject } from '@angular/core';
import { Announcer } from './announcement';

/**
 * Visually hidden live region bound to the shared `Announcer` (T-FE-036).
 * Polite announcements render as `role="status"`; assertive ones as
 * `role="alert"`. RTL-independent: no visual content is rendered.
 */
@Component({
  selector: 'np-live-region',
  templateUrl: './live-region.html',
  styleUrl: './live-region.scss',
})
export class NpLiveRegion {
  private readonly announcer = inject(Announcer);

  protected get announcement() {
    return this.announcer.current();
  }

  protected get livePoliteness(): string {
    return this.announcement?.politeness ?? 'polite';
  }

  protected get statusRole(): string {
    return this.announcement?.politeness === 'assertive' ? 'alert' : 'status';
  }
}
