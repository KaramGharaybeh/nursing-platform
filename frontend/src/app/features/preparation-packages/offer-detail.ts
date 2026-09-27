import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import type { PreparationPackageOfferDetailDto } from '../../core/api/generated/models/preparation-package-offer-detail-dto';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import { LocalizationService } from '../../core/i18n/localization.service';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { canonicalRoutePath } from '../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

@Component({
  selector: 'np-offer-detail',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './offer-detail.html',
  styleUrl: './offer-detail.scss',
})
export class OfferDetail implements OnInit {
  private readonly api = inject(PreparationPackageOffersApi);
  private readonly route = inject(ActivatedRoute);
  protected readonly i18n = inject(LocalizationService);

  protected readonly backPath = canonicalRoutePath('PREPARATION_PACKAGES_OFFERS');
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly offer = signal<PreparationPackageOfferDetailDto | undefined>(undefined);
  protected readonly notFound = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected hasSummary(): boolean {
    return (this.offer()?.summary?.trim() ?? '') !== '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    const offerSlug = this.route.snapshot.paramMap.get('offerSlug') ?? '';
    try {
      const loaded = await firstValueFrom(this.api.getOffer(offerSlug));
      this.offer.set(loaded);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notFound.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
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

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
