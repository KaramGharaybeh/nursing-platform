import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { NpLiveRegion } from '../../shared/ui/announcement';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import type { PaginatedResultOfPreparationPackageOfferListItemDto } from '../../core/api/generated/models/paginated-result-of-preparation-package-offer-list-item-dto';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination, resolveListState, type NpListState } from '../../shared/ui/pagination';
import { OfferCard } from './offer-card';

const PAGE_SIZE = 20;

@Component({
  selector: 'np-offers-list',
  imports: [LoadingErrorRetry, NpEmptyState, NpLiveRegion, NpPagination, OfferCard],
  templateUrl: './offers-list.html',
  styleUrl: './offers-list.scss',
})
export class OffersList implements OnInit {
  private readonly api = inject(PreparationPackageOffersApi);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly result = signal<PaginatedResultOfPreparationPackageOfferListItemDto | undefined>(undefined);
  protected readonly page = signal(1);

  protected readonly pageSize = PAGE_SIZE;

  ngOnInit(): void {
    void this.load(1);
  }

  protected async retry(): Promise<void> {
    await this.load(this.page());
  }

  protected async loadPage(page: number): Promise<void> {
    await this.load(page);
  }

  protected listState(): NpListState {
    const current = this.result();
    return resolveListState((current?.items.length ?? 0) > 0, false);
  }

  private async load(page: number): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const result = await firstValueFrom(this.api.listOffers({ page, pageSize: PAGE_SIZE }));
      this.result.set(result);
      this.page.set(result.page);
      if (result.items.length === 0 && result.totalCount > 0 && result.page !== result.totalPages) {
        await this.load(Math.max(1, result.totalPages));
        return;
      }
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
    }
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
