import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceProduct } from '../../core/api/commerce-payments-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { formatMoney } from '../../shared/money';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

const GENERIC_RETRY_COPY = "We couldn't load this product. Try again.";

@Component({
  selector: 'np-product-detail',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetailScreen implements OnInit {
  private readonly api = inject(CommercePaymentsApi);
  private readonly route = inject(ActivatedRoute);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly product = signal<CommerceProduct | undefined>(undefined);
  protected readonly unavailable = signal(false);

  protected readonly backPath = canonicalRoutePath('COMMERCE_PRODUCTS');

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected price(): string {
    const current = this.product();
    if (current === undefined) {
      return '';
    }
    return formatMoney(current.unitAmountMinor, current.currency);
  }

  private productId(): string {
    return this.route.snapshot.paramMap.get('productId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.product.set(undefined);
    this.unavailable.set(false);
    try {
      const loaded = await firstValueFrom(this.api.getProduct(this.productId()));
      this.product.set(loaded);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
      }
    }
  }

  private isNotFound(error: unknown): boolean {
    return (
      typeof error === 'object' &&
      error !== null &&
      'status' in error &&
      (error as { status?: unknown }).status === 404
    );
  }

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: GENERIC_RETRY_COPY, detail: '' };
  }
}
