import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { NpLanguageSwitcher } from '../../../shared/ui/language-switcher';
import { VerifyEmailApi } from './verify-email-api';

@Component({
  selector: 'np-verify-email',
  imports: [RouterLink, NpLanguageSwitcher],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.scss',
})
export class VerifyEmail implements OnInit {
  private readonly verifyEmailApi = inject(VerifyEmailApi);
  protected readonly i18n = inject(LocalizationService);
  private readonly verificationToken: string =
    (inject(ActivatedRoute).snapshot.queryParamMap.get('token') ?? '').trim();

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
  protected readonly publicOffersPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly signUpPath = canonicalRoutePath('AUTH_SIGN_UP');

  protected readonly isLoading = signal(false);
  protected readonly successMessage = signal('');
  private readonly normalizedError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected readonly isTokenMissing = computed(() => this.verificationToken === '');

  protected readonly tokenMissingMessage = computed(() =>
    this.isTokenMissing() ? this.i18n.t('verify.missing') : '',
  );

  protected readonly loadingMessage = computed(() =>
    !this.isTokenMissing() && this.isLoading() ? this.i18n.t('verify.loading') : '',
  );

  protected readonly backendFailureMessage = computed(() => {
    const error = this.normalizedError();
    if (error === undefined) {
      return '';
    }
    if (error.kind === 'validation') {
      return this.i18n.backendErrorCopy(error.detail, 'verify.failure');
    }
    return this.i18n.backendErrorCopy(error.detail, 'verify.failure');
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
      this.successMessage.set(this.i18n.t('verify.success'));
    } catch (error: unknown) {
      this.normalizedError.set(
        this.i18n.safeBackendError(normalizeProblemDetails(this.errorBody(error)), {}),
      );
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
