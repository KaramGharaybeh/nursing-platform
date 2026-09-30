import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder, CommerceOrderPage } from '../../core/api/commerce-payments-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { TranslationKey } from '../../core/i18n/translations';
import { LocalizationService } from '../../core/i18n/localization.service';
import { buildCommerceOrderDetailPath, canonicalRoutePath } from '../../core/routing/canonical-routes';
import { formatMoney } from '../../shared/money';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination } from '../../shared/ui/pagination';
import { orderStatusLabel } from './order-status';

@Component({
  selector: 'np-order-list',
  imports: [DatePipe, RouterLink, NpEmptyState, LoadingErrorRetry, NpPagination],
  templateUrl: './order-list.html',
  styleUrl: './order-list.scss',
})
export class OrderListScreen implements OnInit {
  private readonly api = inject(CommercePaymentsApi);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly page = signal<CommerceOrderPage | undefined>(undefined);
  protected readonly pageNumber = signal(1);
  protected readonly productsPath = canonicalRoutePath('COMMERCE_PRODUCTS');

  ngOnInit(): void {
    void this.load();
  }

  protected detailPath(id: string): string {
    return buildCommerceOrderDetailPath(id);
  }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected status(status: CommerceOrder['status']): string {
    return orderStatusLabel(status, {
      PendingPayment: this.i18n.t('com.statusPendingPayment'),
      Paid: this.i18n.t('com.statusPaid'),
      Failed: this.i18n.t('com.statusFailed'),
      Cancelled: this.i18n.t('com.statusCancelled'),
      Expired: this.i18n.t('com.statusExpired'),
    });
  }

  protected title(order: CommerceOrder): string {
    return order.items.map((item) => item.title).join(', ');
  }

  protected total(order: CommerceOrder): string {
    return formatMoney(order.totalAmountMinor, order.currency);
  }

  protected async changePage(page: number): Promise<void> {
    this.pageNumber.set(page);
    await this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.page.set(undefined);
    try {
      const page = await firstValueFrom(this.api.listOrders(this.pageNumber()));
      this.page.set(page);
      this.pageNumber.set(page.page);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      const normalized = normalizeProblemDetails(
        typeof error === 'object' && error !== null && 'error' in error
          ? (error as { error?: unknown }).error : error,
      );
      this.state.set({ kind: 'error', error: { ...normalized, title: this.i18n.t('com.listLoadError'), detail: '' }, canRetry: true });
    }
  }
}
