import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { VerifyEmailApi } from './verify-email-api';

const MISSING_TOKEN_MESSAGE = 'This verification link is invalid or missing.';

const LOADING_MESSAGE = 'Verifying email…';

const SUCCESS_MESSAGE = 'Email verified successfully.';

const FAILURE_FALLBACK = 'Email verification failed.';

@Component({
  selector: 'np-verify-email',
  imports: [RouterLink],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.scss',
})
export class VerifyEmail implements OnInit {
  private readonly verifyEmailApi = inject(VerifyEmailApi);
  private readonly verificationToken: string =
    (inject(ActivatedRoute).snapshot.queryParamMap.get('token') ?? '').trim();

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');

  protected readonly isLoading = signal(false);
  protected readonly successMessage = signal('');
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly isTokenMissing = computed(() => this.verificationToken === '');

  protected readonly tokenMissingMessage = computed(() =>
    this.isTokenMissing() ? MISSING_TOKEN_MESSAGE : '',
  );

  protected readonly loadingMessage = computed(() =>
    !this.isTokenMissing() && this.isLoading() ? LOADING_MESSAGE : '',
  );

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined) {
      return '';
    }
    if (error.kind === 'validation') {
      return error.detail.trim() !== '' ? error.detail : FAILURE_FALLBACK;
    }
    return error.detail.trim() !== '' ? error.detail : FAILURE_FALLBACK;
  });

  async ngOnInit(): Promise<void> {
    if (this.isTokenMissing()) {
      return;
    }

    this.isLoading.set(true);
    this.normalizedError.set(undefined);
    this.successMessage.set('');

    try {
      await firstValueFrom(this.verifyEmailApi.verifyEmail({ token: this.verificationToken }));
      this.successMessage.set(SUCCESS_MESSAGE);
    } catch (error: unknown) {
      this.normalizedError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isLoading.set(false);
    }
  }

  private errorBody(error: unknown): unknown {
    if (typeof error !== 'object' || error === null) {
      return error;
    }
    if ('error' in error) {
      return (error as { error?: unknown }).error;
    }
    return error;
  }
}
