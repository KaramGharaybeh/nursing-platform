import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceProductPage } from '../../core/api/commerce-payments-api';
import type { CommerceProduct } from '../../core/api/commerce-payments-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildCommerceProductDetailPath } from '../../core/routing/canonical-routes';
import { formatMoney } from '../../shared/money';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination } from '../../shared/ui/pagination';

const GENERIC_RETRY_COPY = "We couldn't load products. Try again.";

@Component({
  selector: 'np-product-list',
  imports: [LoadingErrorRetry, NpEmptyState, NpPagination, RouterLink],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductListScreen implements OnInit {
  private readonly api = inject(CommercePaymentsApi);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly page = signal<CommerceProductPage | undefined>(undefined);
  protected readonly pageNumber = signal(1);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected async loadProductsPage(page: number): Promise<void> {
    this.pageNumber.set(page);
    await this.load();
  }

  protected detailPath(productId: string): string {
    return buildCommerceProductDetailPath(productId);
  }

  protected priceOf(product: CommerceProduct): string {
    return formatMoney(product.unitAmountMinor, product.currency);
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const result = await firstValueFrom(this.api.listProducts(this.pageNumber()));
      this.page.set(result);
      this.pageNumber.set(result.page);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
    }
  }

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: GENERIC_RETRY_COPY, detail: '' };
  }
}
