import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder, CommerceProduct } from '../../core/api/commerce-payments-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import {
  buildCommerceProductDetailPath,
  canonicalRoutePath,
} from '../../core/routing/canonical-routes';
import { formatMoney } from '../../shared/money';
import type { TranslationKey } from '../../core/i18n/translations';
import { LocalizationService } from '../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

type CreateError = { readonly kind: 'conflict' } | { readonly kind: 'generic' };

@Component({
  selector: 'np-checkout',
  imports: [DatePipe, LoadingErrorRetry, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class CheckoutScreen implements OnInit {
  private readonly api = inject(CommercePaymentsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly product = signal<CommerceProduct | undefined>(undefined);
  protected readonly missingContext = signal(false);
  protected readonly unavailable = signal(false);
  protected readonly creating = signal(false);
  protected readonly order = signal<CommerceOrder | undefined>(undefined);
  protected readonly createError = signal<CreateError | undefined>(undefined);

  protected readonly backPath = canonicalRoutePath('COMMERCE_PRODUCTS');

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected async create(): Promise<void> {
    if (this.creating()) {
      return;
    }
    this.creating.set(true);
    this.createError.set(undefined);
    try {
      const created = await firstValueFrom(
        this.api.createOrder({ productId: this.productId() }),
      );
      this.order.set(created);
    } catch (error: unknown) {
      this.createError.set(this.statusOf(error) === 409 ? { kind: 'conflict' } : { kind: 'generic' });
    } finally {
      this.creating.set(false);
    }
  }

  protected async retryCreate(): Promise<void> {
    this.createError.set(undefined);
    await this.create();
  }

  protected async cancel(): Promise<void> {
    await this.router.navigateByUrl(buildCommerceProductDetailPath(this.productId()));
  }

  protected price(): string {
    const current = this.product();
    if (current === undefined) {
      return '';
    }
    return formatMoney(current.unitAmountMinor, current.currency);
  }

  protected orderTotal(): string {
    const created = this.order();
    if (created === undefined) {
      return '';
    }
    return formatMoney(created.totalAmountMinor, created.currency);
  }

  protected orderTitle(): string {
    return this.order()?.items[0]?.title ?? '';
  }

  protected orderIsPendingPayment(): boolean {
    return this.order()?.status === 'PendingPayment';
  }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected createErrorCopy(): string {
    return this.createError()?.kind === 'conflict'
      ? this.i18n.t('com.createConflict')
      : this.i18n.t('com.createRetry');
  }

  private productId(): string {
    return this.route.snapshot.queryParamMap.get('productId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.product.set(undefined);
    this.missingContext.set(false);
    this.unavailable.set(false);
    if (this.productId().trim() === '') {
      this.missingContext.set(true);
      this.state.set({ kind: 'ready' });
      return;
    }
    try {
      const loaded = await firstValueFrom(this.api.getProduct(this.productId()));
      this.product.set(loaded);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.statusOf(error) === 404) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
      }
    }
  }

  private statusOf(error: unknown): number | undefined {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status?: unknown }).status;
      return typeof status === 'number' ? status : undefined;
    }
    return undefined;
  }

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: this.i18n.t('com.checkoutLoadError'), detail: '' };
  }
}
