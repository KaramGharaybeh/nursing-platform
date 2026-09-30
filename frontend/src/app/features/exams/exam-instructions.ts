import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CdkTrapFocus } from '@angular/cdk/a11y';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamSessionStart } from '../../core/api/exams-api';
import type { ExamAttemptDto } from '../../core/api/generated/models/exam-attempt-dto';
import type { ExamDetail as ExamDetailModel } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsDetailPath, buildExamsSessionPath } from '../../core/routing/canonical-routes';
import { Announcer, NpLiveRegion } from '../../shared/ui/announcement';
import { LocalizationService } from '../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

type ResumeState = 'loading' | 'ready' | 'error';

@Component({
  selector: 'np-exam-instructions',
  imports: [CdkTrapFocus, LoadingErrorRetry, NpLiveRegion, RouterLink],
  templateUrl: './exam-instructions.html',
  styleUrl: './exam-instructions.scss',
  host: {
    '(keydown.escape)': 'escapeModal()',
  },
})
export class ExamInstructions implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);
  // Confirmation state is a signal (not the plain TwoStepConfirmation helper) so the
  // zoneless template re-renders the Start/Resume modal when it changes. Semantics stay
  // identical: idle -> confirming on request, back to idle on cancel/confirm, and
  // confirmAction authorizes exactly one Start/Resume POST from confirming.
  private readonly confirmationState = signal<'idle' | 'confirming'>('idle');

  @ViewChild('startTrigger') private readonly startTrigger?: ElementRef<HTMLButtonElement>;
  @ViewChild('resumeTrigger') private readonly resumeTrigger?: ElementRef<HTMLButtonElement>;
  @ViewChild('modalCancel') private readonly modalCancel?: ElementRef<HTMLButtonElement>;

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly notFound = signal(false);
  protected readonly exam = signal<ExamDetailModel | undefined>(undefined);
  protected readonly resumeState = signal<ResumeState>('loading');
  protected readonly resumeAttempt = signal<ExamAttemptDto | undefined>(undefined);
  protected readonly starting = signal(false);
  protected readonly startError = signal(false);

  protected backPath = '';

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected async retryAttempts(): Promise<void> {
    await this.probeResume();
  }

  protected hasInstructions(): boolean {
    return (this.exam()?.instructions?.trim() ?? '') !== '';
  }

  protected requiresPurchase(): boolean {
    const exam = this.exam();
    return exam !== undefined && exam.isFree === false && exam.canStart === false;
  }

  protected isStartable(): boolean {
    return this.exam()?.canStart === true;
  }

  protected isResuming(): boolean {
    return this.resumeAttempt() !== undefined;
  }

  protected isConfirming(): boolean {
    return this.confirmationState() === 'confirming';
  }

  protected modalOpen(): boolean {
    return this.isConfirming();
  }

  protected requestAction(): void {
    if (this.isConfirming() || this.starting()) {
      return;
    }
    this.startError.set(false);
    this.confirmationState.set('confirming');
    setTimeout(() => this.modalCancel?.nativeElement.focus(), 0);
  }

  protected cancelAction(): void {
    if (this.starting()) {
      return;
    }
    const resuming = this.isResuming();
    this.confirmationState.set('idle');
    setTimeout(() => {
      const trigger = resuming ? this.resumeTrigger : this.startTrigger;
      trigger?.nativeElement.focus();
    }, 0);
  }

  protected escapeModal(): void {
    if (this.isConfirming() && !this.starting()) {
      this.cancelAction();
    }
  }

  protected async confirmAction(): Promise<void> {
    if (this.confirmationState() !== 'confirming' || this.starting()) {
      return;
    }
    this.confirmationState.set('idle');
    this.starting.set(true);
    this.startError.set(false);
    let session: ExamSessionStart | undefined = undefined;
    try {
      session = await firstValueFrom(this.api.startExamSession(this.examId()));
      this.announcer.announce(this.i18n.t('exams.sessionStarted'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notFound.set(true);
      } else if (this.isConflict(error)) {
        await this.reconcileAfterConflict();
      } else {
        this.startError.set(true);
      }
    } finally {
      this.starting.set(false);
    }
    if (session !== undefined) {
      await this.router.navigateByUrl(buildExamsSessionPath(session.examId, session.sessionId));
    }
  }

  private examId(): string {
    return this.route.snapshot.paramMap.get('examId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    this.startError.set(false);
    this.backPath = buildExamsDetailPath(this.examId());
    try {
      const loaded = await firstValueFrom(this.api.getExam(this.examId()));
      this.exam.set(loaded);
      this.state.set({ kind: 'ready' });
      if (loaded.canStart) {
        await this.probeResume();
      } else {
        this.resumeState.set('ready');
      }
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notFound.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
      }
    }
  }

  private async probeResume(): Promise<void> {
    this.resumeState.set('loading');
    this.resumeAttempt.set(undefined);
    try {
      const match = await this.api.findResumableAttempt(this.examId());
      this.resumeAttempt.set(match);
      this.resumeState.set('ready');
    } catch {
      this.resumeState.set('error');
    }
  }

  private async reconcileAfterConflict(): Promise<void> {
    try {
      const reloaded = await firstValueFrom(this.api.getExam(this.examId()));
      this.exam.set(reloaded);
      if (reloaded.canStart) {
        this.startError.set(true);
      }
    } catch {
      this.startError.set(true);
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
