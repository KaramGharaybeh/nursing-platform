import { Component, EventEmitter, Input, Output } from '@angular/core';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';

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
  @Input() loadingLabel = 'Loading';
  @Input() fallbackErrorTitle = 'Something went wrong';
  @Input() retryLabel = 'Try again';

  @Output() readonly retryRequested = new EventEmitter<void>();

  protected get errorTitle(): string {
    if (this.state.kind !== 'error') {
      return this.fallbackErrorTitle;
    }

    const title = this.state.error.title.trim();
    return title === '' ? this.fallbackErrorTitle : title;
  }

  protected get errorDetail(): string | undefined {
    if (this.state.kind !== 'error') {
      return undefined;
    }

    const detail = this.state.error.detail.trim();
    return detail === '' ? undefined : detail;
  }

  protected requestRetry(): void {
    this.retryRequested.emit();
  }
}
