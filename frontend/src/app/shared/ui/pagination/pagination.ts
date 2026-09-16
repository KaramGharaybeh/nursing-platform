import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

export type NpListState = 'results' | 'empty' | 'no-results';

/**
 * Selects the list presentation state without owning any query state
 * (T-FE-038 filter/page contract): items present always render results;
 * zero items with no active query is empty (SYS-006); zero items with an
 * active query is no-results (SYS-007, filters preserved by the consumer).
 */
export function resolveListState(hasItems: boolean, hasActiveQuery: boolean): NpListState {
  if (hasItems) {
    return 'results';
  }
  return hasActiveQuery ? 'no-results' : 'empty';
}

/**
 * Shared pagination navigation primitive (T-FE-038, HD-G1/HD-G2).
 *
 * Presentation only: renders Previous / status / Next inside a nav landmark
 * and emits 1-based `pageRequested` values. It never fetches data, never reads
 * routes or filters, never recomputes totalPages, and never renders when
 * `totalPages <= 1` (zero-result and single-page sets). Consumers own filter
 * state and reload behavior: filter/search change and Clear/Reset request
 * page 1; retry preserves the current page; loading uses full swap via
 * `np-loading-error-retry`; empty/no-results compose `np-empty-state`.
 */
@Component({
  selector: 'np-pagination',
  imports: [MatButtonModule],
  templateUrl: './pagination.html',
  styleUrl: './pagination.scss',
})
export class NpPagination {
  @Input() page = 1;
  @Input() totalPages = 0;
  @Input() totalCount = 0;
  @Input() itemLabel = 'total';
  @Input() ariaLabel = 'Pagination';

  @Output() readonly pageRequested = new EventEmitter<number>();

  protected get isVisible(): boolean {
    return this.totalPages > 1;
  }

  protected get isPreviousDisabled(): boolean {
    return this.page <= 1;
  }

  protected get isNextDisabled(): boolean {
    return this.page >= this.totalPages;
  }

  protected get statusText(): string {
    return `Page ${this.page} of ${this.totalPages} · ${this.totalCount} ${this.itemLabel}`;
  }

  protected requestPrevious(): void {
    if (!this.isPreviousDisabled) {
      this.pageRequested.emit(this.page - 1);
    }
  }

  protected requestNext(): void {
    if (!this.isNextDisabled) {
      this.pageRequested.emit(this.page + 1);
    }
  }
}
