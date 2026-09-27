import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamSession } from '../../core/api/exams-api';
import type { ExamSessionQuestion } from '../../core/api/exams-api';
import type { ExamSessionResult } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsDetailPath, buildExamsResultPath } from '../../core/routing/canonical-routes';
import { TwoStepConfirmation } from '../../shared/ui/confirmation';
import { Announcer, NpLiveRegion } from '../../shared/ui/announcement';
import { LocalizationService } from '../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

const STATUS_IN_PROGRESS = 'InProgress';
const NEAR_EXPIRY_SECONDS = 300;

@Component({
  selector: 'np-exam-session',
  imports: [LoadingErrorRetry, NpLiveRegion, RouterLink],
  templateUrl: './exam-session.html',
  styleUrl: './exam-session.scss',
})
export class ExamSessionScreen implements OnInit, OnDestroy {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);
  private readonly confirmation = new TwoStepConfirmation();
  private timer: ReturnType<typeof setInterval> | undefined = undefined;
  private warned = false;

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly notFound = signal(false);
  protected readonly session = signal<ExamSession | undefined>(undefined);
  protected readonly currentIndex = signal(0);
  protected readonly localSelection = signal<Readonly<Record<string, string>>>({});
  protected readonly saving = signal(false);
  protected readonly saveError = signal(false);
  protected readonly submitting = signal(false);
  protected readonly result = signal<ExamSessionResult | undefined>(undefined);
  protected readonly remainingDisplay = signal(0);

  protected backPath = '';

  protected resultPath(): string {
    return buildExamsResultPath(this.examId(), this.session()?.id ?? this.sessionId());
  }

  ngOnInit(): void {
    void this.load();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected currentQuestion(): ExamSessionQuestion | undefined {
    return this.session()?.items[this.currentIndex()];
  }

  protected isActive(): boolean {
    return this.session()?.status === STATUS_IN_PROGRESS && this.result() === undefined;
  }

  protected isTerminal(): boolean {
    const session = this.session();
    return session !== undefined && session.status !== STATUS_IN_PROGRESS;
  }

  protected persistedSelection(questionId: string): string | undefined {
    return (
      this.session()?.items.find((item) => item.examSessionQuestionId === questionId)
        ?.selectedExamSessionAnswerOptionId ?? undefined
    );
  }

  protected effectiveSelection(questionId: string): string | undefined {
    return this.localSelection()[questionId] ?? this.persistedSelection(questionId);
  }

  protected hasUnsavedChange(questionId: string): boolean {
    const local = this.localSelection()[questionId];
    return local !== undefined && local !== this.persistedSelection(questionId);
  }

  protected selectOption(optionId: string): void {
    const question = this.currentQuestion();
    if (question === undefined || !this.isActive()) {
      return;
    }
    this.localSelection.set({ ...this.localSelection(), [question.examSessionQuestionId]: optionId });
  }

  protected async save(): Promise<void> {
    const question = this.currentQuestion();
    const selected = question === undefined ? undefined : this.localSelection()[question.examSessionQuestionId];
    const sessionId = this.sessionId();
    if (question === undefined || selected === undefined || this.saving() || !this.isActive()) {
      return;
    }
    this.saving.set(true);
    this.saveError.set(false);
    try {
      const updated = await firstValueFrom(
        this.api.saveExamSessionAnswers(sessionId, [
          { examSessionQuestionId: question.examSessionQuestionId, selectedExamSessionAnswerOptionId: selected },
        ]),
      );
      this.applySession(updated, { preserveLocalSelection: false });
      this.announcer.announce(this.i18n.t('session.announceSaved'));
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notFound.set(true);
      } else {
        await this.reconcile();
        this.saveError.set(true);
      }
    } finally {
      this.saving.set(false);
    }
  }

  protected previous(): void {
    this.goTo(this.currentIndex() - 1);
  }

  protected next(): void {
    this.goTo(this.currentIndex() + 1);
  }

  protected goTo(index: number): void {
    const total = this.session()?.items.length ?? 0;
    if (total === 0) {
      return;
    }
    this.currentIndex.set(Math.min(Math.max(index, 0), total - 1));
  }

  protected unansweredCount(): number {
    const session = this.session();
    if (session === undefined) {
      return 0;
    }
    return session.items.filter((item) => this.persistedSelection(item.examSessionQuestionId) === undefined).length;
  }

  protected isConfirming(): boolean {
    return this.confirmation.state === 'confirming';
  }

  protected requestSubmit(): void {
    this.confirmation.request();
  }

  protected cancelSubmit(): void {
    this.confirmation.cancel();
  }

  protected async confirmSubmit(): Promise<void> {
    if (!this.confirmation.confirm() || this.submitting()) {
      return;
    }
    const sessionId = this.sessionId();
    if (sessionId === '') {
      return;
    }
    this.submitting.set(true);
    try {
      const submitted = await firstValueFrom(this.api.submitExamSession(sessionId));
      this.result.set(submitted);
      this.stopTimer();
      this.announcer.announce(submitted.passed ? this.i18n.t('session.announcePassed') : this.i18n.t('session.announceFailed'));
      await this.reconcile();
    } catch {
      await this.reconcile();
    } finally {
      this.submitting.set(false);
    }
  }

  protected tick(): void {
    if (!this.isActive()) {
      return;
    }
    const next = this.remainingDisplay() - 1;
    this.remainingDisplay.set(Math.max(0, next));
    if (this.remainingDisplay() <= NEAR_EXPIRY_SECONDS && !this.warned && this.remainingDisplay() > 0) {
      this.warned = true;
      this.announcer.announce(this.i18n.t('session.expiryWarning'));
    }
    if (this.remainingDisplay() <= 0) {
      this.stopTimer();
      void this.reconcile();
    }
  }

  protected formattedRemaining(): string {
    const total = Math.max(0, this.remainingDisplay());
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  protected isWarning(): boolean {
    return (
      this.isActive() && this.remainingDisplay() <= NEAR_EXPIRY_SECONDS && this.remainingDisplay() > 0
    );
  }

  private sessionId(): string {
    return this.route.snapshot.paramMap.get('sessionId') ?? '';
  }

  private examId(): string {
    return this.route.snapshot.paramMap.get('examId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    this.result.set(undefined);
    this.backPath = buildExamsDetailPath(this.examId());
    try {
      const loaded = await firstValueFrom(this.api.getExamSession(this.sessionId()));
      this.applySession(loaded, { preserveLocalSelection: true });
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

  private applySession(loaded: ExamSession, options: { preserveLocalSelection: boolean }): void {
    this.session.set(loaded);
    if (!options.preserveLocalSelection) {
      this.localSelection.set({});
    }
    this.warned = loaded.remainingSeconds <= NEAR_EXPIRY_SECONDS;
    if (this.warned && loaded.status === STATUS_IN_PROGRESS && loaded.remainingSeconds > 0) {
      this.announcer.announce(this.i18n.t('session.expiryWarning'));
    }
    this.remainingDisplay.set(Math.max(0, loaded.remainingSeconds));
    this.restartTimer(loaded.status === STATUS_IN_PROGRESS && loaded.remainingSeconds > 0);
    this.selectResumeIndex();
  }

  private selectResumeIndex(): void {
    const items = this.session()?.items ?? [];
    const firstUnanswered = items.findIndex(
      (item) => this.persistedSelection(item.examSessionQuestionId) === undefined,
    );
    this.currentIndex.set(firstUnanswered === -1 ? 0 : firstUnanswered);
  }

  private async reconcile(): Promise<void> {
    try {
      const reloaded = await firstValueFrom(this.api.getExamSession(this.sessionId()));
      this.applySession(reloaded, { preserveLocalSelection: true });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notFound.set(true);
      } else {
        this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
      }
    }
  }

  private restartTimer(running: boolean): void {
    this.stopTimer();
    if (running) {
      this.timer = setInterval(() => this.tick(), 1000);
    }
  }

  private stopTimer(): void {
    if (this.timer !== undefined) {
      clearInterval(this.timer);
      this.timer = undefined;
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
