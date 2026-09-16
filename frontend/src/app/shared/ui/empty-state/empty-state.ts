import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

export type NpEmptyStateKind = 'empty' | 'no-results';

let nextEmptyStateId = 0;

/**
 * Shared presentational empty / no-results state primitive (T-FE-035,
 * SYS-006/SYS-007, HD-SYS1/HD-SYS2).
 *
 * - `empty`: the underlying collection genuinely has zero records. An action
 *   (e.g. Add/Upload/Manage) is optional and supplied by the consumer.
 * - `no-results`: records may exist, but the active search/filter/query
 *   matches zero. The consumer owns query state; this component only presents
 *   and emits `actionRequested` (e.g. Clear filters). It never mutates filters,
 *   never claims a global empty, and never offers a create CTA unless the
 *   consumer explicitly supplies that action label.
 *
 * Static screen content: intentionally no live-region announcement.
 * Restricted/denied presentation stays with the shipped access-denied screen
 * (HD-SYS3); loading/error stay with `np-loading-error-retry`.
 */
@Component({
  selector: 'np-empty-state',
  imports: [MatButtonModule],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
})
export class NpEmptyState {
  @Input() kind: NpEmptyStateKind = 'empty';
  @Input() title = '';
  @Input() description = '';
  @Input() actionLabel = '';

  @Output() readonly actionRequested = new EventEmitter<void>();

  protected readonly titleId = `np-empty-state-title-${++nextEmptyStateId}`;

  protected get hasAction(): boolean {
    return this.actionLabel.trim() !== '';
  }

  protected get hasDescription(): boolean {
    return this.description.trim() !== '';
  }

  protected requestAction(): void {
    this.actionRequested.emit();
  }
}
