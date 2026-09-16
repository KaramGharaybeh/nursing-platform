import { Injectable, signal } from '@angular/core';

export type AnnouncementPoliteness = 'polite' | 'assertive';

export interface Announcement {
  readonly sequence: number;
  readonly text: string;
  readonly politeness: AnnouncementPoliteness;
}

/**
 * Shared screen-reader announcement primitive (T-FE-036).
 *
 * Features announce meaningful async state changes (item deleted, save
 * completed, already-missing resource) through `announce()` and render a
 * single `NpLiveRegion` to expose them. Announce sparingly: routine
 * interactions whose visual state already conveys the outcome (navigation,
 * form field edits, opening a form) must not announce.
 *
 * Identical consecutive messages re-announce because every call advances
 * `sequence` and briefly clears the rendered text first. Rapid successive
 * calls resolve to the latest message; empty messages are ignored.
 */
@Injectable({ providedIn: 'root' })
export class Announcer {
  private nextSequence = 0;

  private readonly currentAnnouncement = signal<Announcement | undefined>(undefined);

  readonly current = this.currentAnnouncement.asReadonly();

  announce(text: string, politeness: AnnouncementPoliteness = 'polite'): void {
    const message = text.trim();
    if (message === '') {
      return;
    }
    this.nextSequence += 1;
    const sequence = this.nextSequence;
    this.currentAnnouncement.set(undefined);
    queueMicrotask(() => {
      if (this.nextSequence === sequence) {
        this.currentAnnouncement.set({ sequence, text: message, politeness });
      }
    });
  }

  clear(): void {
    this.currentAnnouncement.set(undefined);
  }
}
