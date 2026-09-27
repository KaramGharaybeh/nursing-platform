import { Component, OnInit, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { ExamsApi } from '../../../core/api/exams-api';
import type { PackageBenefitRightSummaryDto } from '../../../core/api/generated/models/package-benefit-right-summary-dto';
import type { PackageExamSessionStateDto } from '../../../core/api/generated/models/package-exam-session-state-dto';
import { buildExamsSessionPath, buildPreparationPackageReportPath } from '../../../core/routing/canonical-routes';
import { TwoStepConfirmation } from '../../../shared/ui/confirmation';
import { Announcer, NpLiveRegion } from '../../../shared/ui/announcement';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

type PackageExamUiState =
  | 'loading'
  | 'start'
  | 'resume'
  | 'completed'
  | 'expired'
  | 'used'
  | 'packageExpired'
  | 'stateError';

@Component({
  selector: 'np-package-exam-section',
  imports: [LoadingErrorRetry, NpLiveRegion, RouterLink],
  templateUrl: './package-exam-section.html',
  styleUrl: './package-exam-section.scss',
})
export class PackageExamSection implements OnInit {
  private readonly api = inject(PreparationPackageEntitlementsApi);
  private readonly examsApi = inject(ExamsApi);
  private readonly router = inject(Router);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);
  private readonly confirmation = new TwoStepConfirmation();

  readonly entitlementId = input.required<string>();
  readonly rights = input<PackageBenefitRightSummaryDto[]>([]);
  readonly entitlementActive = input(true);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly uiState = signal<PackageExamUiState>('loading');
  protected readonly reportSessionId = signal<string | undefined>(undefined);
  protected readonly starting = signal(false);
  protected readonly startError = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retryState(): Promise<void> {
    await this.loadExamState();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected isConfirming(): boolean {
    return this.confirmation.state === 'confirming';
  }

  protected requestAction(): void {
    this.startError.set(false);
    this.confirmation.request();
  }

  protected cancelAction(): void {
    this.confirmation.cancel();
  }

  protected async confirmAction(): Promise<void> {
    if (!this.confirmation.confirm() || this.starting()) {
      return;
    }
    this.starting.set(true);
    this.startError.set(false);
    try {
      const started = await firstValueFrom(this.api.startPackageExamSession(this.entitlementId()));
      this.announcer.announce(this.i18n.t('npp.sessionStarted'));
      await this.router.navigateByUrl(
        buildExamsSessionPath(started.examId, started.sessionId),
      );
    } catch (error: unknown) {
      if (this.isConflict(error)) {
        await this.loadExamState();
        if (this.uiState() === 'start') {
          this.startError.set(true);
        }
      } else {
        this.startError.set(true);
      }
    } finally {
      this.starting.set(false);
    }
  }

  protected reportPath(): string {
    return buildPreparationPackageReportPath(this.reportSessionId() ?? '');
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.uiState.set('loading');
    this.startError.set(false);
    try {
      await this.loadExamState();
      this.state.set({ kind: 'ready' });
    } catch {
      this.state.set({ kind: 'ready' });
      this.uiState.set('stateError');
    }
  }

  private async loadExamState(): Promise<void> {
    const session = await firstValueFrom(this.api.getPackageExamSessionState(this.entitlementId()));
    await this.resolveUiState(session);
  }

  private async resolveUiState(session: PackageExamSessionStateDto): Promise<void> {
    if (session.hasSession && session.sessionId !== null && session.sessionId !== undefined) {
      const status = session.status ?? '';
      if (status === 'Submitted' || status === 'Expired') {
        this.reportSessionId.set(session.sessionId);
        this.uiState.set(status === 'Submitted' ? 'completed' : 'expired');
        return;
      }
      if (status === 'InProgress') {
        const reconciled = await this.reconcileStaleSession(session.sessionId);
        if (reconciled === 'expired') {
          this.reportSessionId.set(session.sessionId);
          this.uiState.set('expired');
          return;
        }
        if (reconciled === 'active') {
          this.uiState.set('resume');
          return;
        }
      }
      this.uiState.set('used');
      return;
    }
    if (this.hasUsableAttemptRight()) {
      this.uiState.set('start');
      return;
    }
    const attemptRight = this.rights().find(
      (right) => right.rightType === 'PackageExamAttemptEligibility',
    );
    if (attemptRight === undefined || !attemptRight.isAvailable) {
      this.uiState.set('used');
      return;
    }
    this.uiState.set('packageExpired');
  }

  private async reconcileStaleSession(sessionId: string): Promise<'active' | 'expired' | 'unknown'> {
    try {
      const authoritative = await firstValueFrom(this.examsApi.getExamSession(sessionId));
      if (authoritative.status === 'Expired') {
        return 'expired';
      }
      if (authoritative.status === 'InProgress' && this.isUnexpired(authoritative.expiresAt)) {
        return 'active';
      }
      return 'unknown';
    } catch {
      return 'unknown';
    }
  }

  private isUnexpired(expiresAt: string): boolean {
    const parsed = Date.parse(expiresAt);
    return Number.isFinite(parsed) && parsed > Date.now();
  }

  private hasUsableAttemptRight(): boolean {
    return (
      this.entitlementActive() &&
      this.rights().some((right) => right.rightType === 'PackageExamAttemptEligibility' && right.isAvailable)
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
}
