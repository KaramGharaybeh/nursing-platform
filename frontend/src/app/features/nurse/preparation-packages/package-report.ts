import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import type { PackageAnalyticalReportDto } from '../../../core/api/generated/models/package-analytical-report-dto';
import type { PackageAnalyticalReportGuidanceItemDto } from '../../../core/api/generated/models/package-analytical-report-guidance-item-dto';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

@Component({
  selector: 'np-package-report',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './package-report.html',
  styleUrl: './package-report.scss',
})
export class PackageReport implements OnInit {
  private readonly api = inject(PreparationPackageEntitlementsApi);
  private readonly route = inject(ActivatedRoute);

  protected readonly backPath = canonicalRoutePath('PREPARATION_PACKAGES_ENTITLEMENTS');
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly report = signal<PackageAnalyticalReportDto | undefined>(undefined);
  protected readonly unavailable = signal(false);
  protected readonly notFinalized = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected guidanceLabel(item: PackageAnalyticalReportGuidanceItemDto): string | undefined {
    if (item.sourceType === 'StudyMaterialVersion') {
      return 'Study material';
    }
    if (item.sourceType === 'PracticeCollectionVersion') {
      return 'Practice collection';
    }
    return undefined;
  }

  protected hasGuidance(): boolean {
    return (this.report()?.guidanceItems.length ?? 0) > 0;
  }

  private sessionId(): string {
    return this.route.snapshot.paramMap.get('sessionId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.unavailable.set(false);
    this.notFinalized.set(false);
    try {
      const loaded = await firstValueFrom(this.api.getPackageAnalyticalReport(this.sessionId()));
      this.report.set(loaded);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else if (this.isConflict(error)) {
        this.notFinalized.set(true);
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

  private isConflict(error: unknown): boolean {
    return (
      typeof error === 'object' &&
      error !== null &&
      'status' in error &&
      (error as { status?: unknown }).status === 409
    );
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
