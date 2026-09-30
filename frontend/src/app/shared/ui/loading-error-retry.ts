import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { LocalizationService } from '../../core/i18n/localization.service';

export type LoadingErrorRetryState =
  | { readonly kind: 'ready' }
  | { readonly kind: 'loading' }
  | {
      readonly kind: 'error';
      readonly error: NormalizedProblemDetails;
      readonly canRetry?: boolean;
    };

@Component({
  selector: 'np-loading-error-retry',
  templateUrl: './loading-error-retry.html',
  styleUrl: './loading-error-retry.scss',
})
export class LoadingErrorRetry {
  @Input() state: LoadingErrorRetryState = { kind: 'ready' };
  @Input() loadingLabel?: string;
  @Input() fallbackErrorTitle?: string;
  @Input() retryLabel?: string;
  protected readonly i18n = inject(LocalizationService);

  @Output() readonly retryRequested = new EventEmitter<void>();

  protected get resolvedLoadingLabel(): string {
    return this.loadingLabel ?? this.i18n.t('common.loading');
  }

  protected get resolvedFallbackErrorTitle(): string {
    return this.fallbackErrorTitle ?? this.i18n.t('common.genericError');
  }

  protected get resolvedRetryLabel(): string {
    return this.retryLabel ?? this.i18n.t('common.retry');
  }

  protected get errorTitle(): string {
    if (this.state.kind !== 'error' || !this.i18n.isDefaultLocale()) {
      return this.resolvedFallbackErrorTitle;
    }

    const title = this.state.error.title.trim();
    return title === '' ? this.resolvedFallbackErrorTitle : title;
  }

  protected get errorDetail(): string | undefined {
    if (this.state.kind !== 'error' || !this.i18n.isDefaultLocale()) {
      return undefined;
    }

    const detail = this.state.error.detail.trim();
    return detail === '' ? undefined : detail;
  }

  protected requestRetry(): void {
    this.retryRequested.emit();
  }
}
