import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import type { PackageEntitlementDetailDto } from '../../../core/api/generated/models/package-entitlement-detail-dto';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { LocalizationService } from '../../../core/i18n/localization.service';
import type { TranslationKey } from '../../../core/i18n/translations';
import { canonicalRoutePath, buildCanonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';
import { NurseEntitlementRight } from './nurse-entitlement-right';
import { PackageExamSection } from './package-exam-section';

const DETAIL_STATUS_KEYS: Record<string, TranslationKey> = {
  Active: 'npp.statusActive',
  Expired: 'npp.statusExpired',
  Locked: 'npp.statusLocked',
};

@Component({
  selector: 'np-nurse-entitlement-detail',
  imports: [DatePipe, LoadingErrorRetry, NurseEntitlementRight, PackageExamSection, RouterLink],
  templateUrl: './nurse-entitlement-detail.html',
  styleUrl: './nurse-entitlement-detail.scss',
})
export class NurseEntitlementDetail implements OnInit {
  private readonly api = inject(PreparationPackageEntitlementsApi);
  private readonly route = inject(ActivatedRoute);
  protected readonly i18n = inject(LocalizationService);

  protected readonly backPath = canonicalRoutePath('PREPARATION_PACKAGES_ENTITLEMENTS');

  protected statusText(status: string): string {
    const key = DETAIL_STATUS_KEYS[status];
    return key === undefined ? status : this.i18n.t(key);
  }
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly entitlement = signal<PackageEntitlementDetailDto | undefined>(undefined);
  protected readonly notFound = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected hasSummary(): boolean {
    return (this.entitlement()?.purchasedSnapshot?.packageOfferSummary?.trim() ?? '') !== '';
  }

  protected hasPracticeAccess(): boolean {
    return (this.entitlement()?.benefitRights ?? []).some(
      (right) => right.rightType === 'PracticeAccess' && right.isAvailable,
    );
  }

  protected practicePath(): string {
    return buildCanonicalRoutePath('PREPARATION_PACKAGES_PRACTICE', {
      entitlementId: this.route.snapshot.paramMap.get('entitlementId') ?? '',
    });
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    const entitlementId = this.route.snapshot.paramMap.get('entitlementId') ?? '';
    try {
      const loaded = await firstValueFrom(this.api.getMyEntitlement(entitlementId));
      this.entitlement.set(loaded);
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
