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
  template: `
    @if (state.kind === 'loading') {
      <section
        class="np-loading-error-retry np-loading-error-retry-loading"
        role="status"
        aria-live="polite"
        aria-busy="true"
      >
        <span class="np-loading-error-retry-indicator" aria-hidden="true"></span>
        <span>{{ loadingLabel }}</span>
      </section>
    } @else if (state.kind === 'error') {
      <section
        class="np-loading-error-retry np-loading-error-retry-error"
        role="alert"
        aria-live="assertive"
      >
        <h2 class="np-loading-error-retry-title">{{ errorTitle }}</h2>
        @if (errorDetail !== undefined) {
          <p class="np-loading-error-retry-detail">{{ errorDetail }}</p>
        }
        @if (state.canRetry === true) {
          <button class="np-loading-error-retry-retry" type="button" (click)="requestRetry()">
            {{ retryLabel }}
          </button>
        }
      </section>
    } @else {
      <ng-content />
    }
  `,
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
