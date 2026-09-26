import { DatePipe } from '@angular/common';
import { afterNextRender, Component, ElementRef, inject, Injector, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder, CommerceOrderItem } from '../../core/api/commerce-payments-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { formatMoney } from '../../shared/money';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { orderStatusLabel } from './order-status';

const ERROR_COPY = "We couldn't load this order. Try again.";

@Component({
  selector: 'np-order-detail',
  imports: [DatePipe, RouterLink, LoadingErrorRetry],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetailScreen implements OnInit {
  private readonly api = inject(CommercePaymentsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly injector = inject(Injector);
  private readonly confirmationHeading = viewChild<ElementRef<HTMLHeadingElement>>('confirmationHeading');
  private readonly cancelButton = viewChild<ElementRef<HTMLButtonElement>>('cancelButton');
  private readonly pageHeading = viewChild.required<ElementRef<HTMLHeadingElement>>('pageHeading');

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly order = signal<CommerceOrder | undefined>(undefined);
  protected readonly unavailable = signal(false);
  protected readonly confirming = signal(false);
  protected readonly cancelling = signal(false);
  protected readonly cancelMessage = signal<string | undefined>(undefined);
  protected readonly backPath = canonicalRoutePath('COMMERCE_ORDERS');

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> { await this.load(); }

  protected status(status: CommerceOrder['status']): string { return orderStatusLabel(status); }

  protected total(order: CommerceOrder): string { return formatMoney(order.totalAmountMinor, order.currency); }

  protected itemTotal(item: CommerceOrderItem): string {
    return formatMoney(item.lineTotalAmountMinor, item.currency);
  }

  protected confirm(): void {
    if (this.order()?.status !== 'PendingPayment' || this.cancelling()) return;
    this.confirming.set(true);
    afterNextRender(() => this.confirmationHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  protected keep(): void {
    this.confirming.set(false);
    afterNextRender(() => this.cancelButton()?.nativeElement.focus(), { injector: this.injector });
  }

  protected async cancel(): Promise<void> {
    if (!this.confirming() || this.cancelling() || this.order()?.status !== 'PendingPayment') return;
    this.cancelling.set(true);
    this.cancelMessage.set(undefined);
    try {
      await firstValueFrom(this.api.cancelOrder(this.orderId()));
      this.confirming.set(false);
      await this.load();
      afterNextRender(() => this.pageHeading().nativeElement.focus(), { injector: this.injector });
    } catch (error: unknown) {
      this.confirming.set(false);
      this.cancelMessage.set(this.statusOf(error) === 409
        ? 'This order can no longer be cancelled.'
        : "We couldn't cancel this order. Check its current status before trying again.");
      await this.load();
      afterNextRender(() => this.pageHeading().nativeElement.focus(), { injector: this.injector });
    } finally {
      this.cancelling.set(false);
    }
  }

  private orderId(): string { return this.route.snapshot.paramMap.get('orderId') ?? ''; }

  private statusOf(error: unknown): number | undefined {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status?: unknown }).status;
      return typeof status === 'number' ? status : undefined;
    }
    return undefined;
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.order.set(undefined);
    this.unavailable.set(false);
    try {
      this.order.set(await firstValueFrom(this.api.getOrder(this.orderId())));
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.statusOf(error) === 404) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        const normalized = normalizeProblemDetails(
          typeof error === 'object' && error !== null && 'error' in error
            ? (error as { error?: unknown }).error : error,
        );
        this.state.set({ kind: 'error', error: { ...normalized, title: ERROR_COPY, detail: '' }, canRetry: true });
      }
    }
  }
}
