import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CdkTrapFocus } from '@angular/cdk/a11y';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamSession } from '../../core/api/exams-api';
import type { ExamSessionQuestion } from '../../core/api/exams-api';
import type { ExamSessionResult } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsDetailPath, buildExamsResultPath } from '../../core/routing/canonical-routes';
import { Announcer, NpLiveRegion } from '../../shared/ui/announcement';
import { LocalizationService } from '../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

const STATUS_IN_PROGRESS = 'InProgress';
const NEAR_EXPIRY_SECONDS = 300;

type SessionMutation =
  | { kind: 'save'; questionId: string; optionId: string }
  | { kind: 'clear'; questionId: string }
  | { kind: 'flag'; questionId: string };

@Component({
  selector: 'np-exam-session',
  imports: [CdkTrapFocus, LoadingErrorRetry, NpLiveRegion, RouterLink],
  templateUrl: './exam-session.html',
  styleUrl: './exam-session.scss',
  host: {
    '(keydown.escape)': 'escapeModal()',
  },
})
export class ExamSessionScreen implements OnInit, OnDestroy {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);
  // Confirmation state is a signal (not the plain TwoStepConfirmation helper) so the
  // zoneless template re-renders the submit modal when it changes. Semantics stay
  // identical: idle -> confirming on request, back to idle on cancel/confirm, and
  // confirm() authorizes exactly one submit from confirming.
  private readonly confirmationState = signal<'idle' | 'confirming'>('idle');
  private timer: ReturnType<typeof setInterval> | undefined = undefined;
  private warned = false;
  private mutationTail: Promise<void> = Promise.resolve();
  private pendingMutations = 0;
  private failedIntent: SessionMutation | undefined = undefined;

  @ViewChild('submitTrigger') private readonly submitTrigger?: ElementRef<HTMLButtonElement>;
  @ViewChild('confirmCancel') private readonly confirmCancel?: ElementRef<HTMLButtonElement>;

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly notFound = signal(false);
  protected readonly session = signal<ExamSession | undefined>(undefined);
  protected readonly currentIndex = signal(0);
  protected readonly localSelection = signal<Readonly<Record<string, string>>>({});
  protected readonly clearedSelection = signal<Readonly<Record<string, true>>>({});
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
    if (this.clearedSelection()[questionId] === true) {
      return undefined;
    }
    return this.localSelection()[questionId] ?? this.persistedSelection(questionId);
  }

  protected persistedFlag(questionId: string): boolean {
    return (
      this.session()?.items.find((item) => item.examSessionQuestionId === questionId)?.isFlagged ===
      true
    );
  }

  protected selectOption(optionId: string): void {
    const question = this.currentQuestion();
    if (question === undefined || !this.isActive() || this.submitting() || this.isConfirming()) {
      return;
    }
    const questionId = question.examSessionQuestionId;
    const cleared = { ...this.clearedSelection() };
    delete cleared[questionId];
    this.clearedSelection.set(cleared);
    this.localSelection.set({ ...this.localSelection(), [questionId]: optionId });
    this.enqueueMutation({ kind: 'save', questionId, optionId });
  }

  protected clearSelection(): void {
    const question = this.currentQuestion();
    if (question === undefined || !this.isActive() || this.submitting() || this.isConfirming()) {
      return;
    }
    const questionId = question.examSessionQuestionId;
    if (this.effectiveSelection(questionId) === undefined) {
      return;
    }
    const local = { ...this.localSelection() };
    delete local[questionId];
    this.localSelection.set(local);
    this.clearedSelection.set({ ...this.clearedSelection(), [questionId]: true });
    this.enqueueMutation({ kind: 'clear', questionId });
  }

  protected toggleFlag(): void {
    const question = this.currentQuestion();
    if (question === undefined || !this.isActive() || this.submitting() || this.isConfirming()) {
      return;
    }
    this.enqueueMutation({ kind: 'flag', questionId: question.examSessionQuestionId });
  }

  protected async retryMutation(): Promise<void> {
    const intent = this.failedIntent;
    if (intent === undefined || !this.isActive() || this.submitting() || this.isConfirming()) {
      return;
    }
    this.failedIntent = undefined;
    this.enqueueMutation(intent);
    await this.drainMutations();
  }

  protected async previous(): Promise<void> {
    await this.goToQuestion(this.currentIndex() - 1);
  }

  protected async next(): Promise<void> {
    await this.goToQuestion(this.currentIndex() + 1);
  }

  protected async goToQuestion(index: number): Promise<void> {
    const total = this.session()?.items.length ?? 0;
    if (total === 0 || !this.isActive()) {
      return;
    }
    const clamped = Math.min(Math.max(index, 0), total - 1);
    await this.drainMutations();
    if (this.saveError() || !this.isActive()) {
      return;
    }
    this.currentIndex.set(clamped);
  }

  protected navLabel(index: number, item: ExamSessionQuestion): string {
    const states: string[] = [
      this.effectiveSelection(item.examSessionQuestionId) === undefined
        ? this.i18n.t('session.unanswered')
        : this.i18n.t('session.answered'),
    ];
    if (this.persistedFlag(item.examSessionQuestionId)) {
      states.push(this.i18n.t('session.flagged'));
    }
    if (index === this.currentIndex()) {
      states.push(this.i18n.t('session.navCurrent'));
    }
    return this.i18n.tp('session.navItem', { current: index + 1, state: states.join(', ') });
  }

  protected unansweredCount(): number {
    const session = this.session();
    if (session === undefined) {
      return 0;
    }
    return session.items.filter((item) => this.persistedSelection(item.examSessionQuestionId) === undefined).length;
  }

  protected flaggedCount(): number {
    const session = this.session();
    if (session === undefined) {
      return 0;
    }
    return session.items.filter((item) => item.isFlagged === true).length;
  }

  protected isConfirming(): boolean {
    return this.confirmationState() === 'confirming';
  }

  protected modalOpen(): boolean {
    return (this.isConfirming() || this.submitting()) && this.isActive();
  }

  protected async requestSubmit(): Promise<void> {
    if (this.submitting() || this.isConfirming() || !this.isActive()) {
      return;
    }
    await this.drainMutations();
    if (this.saveError() || this.submitting() || this.isConfirming() || !this.isActive()) {
      return;
    }
    this.confirmationState.set('confirming');
    setTimeout(() => this.confirmCancel?.nativeElement.focus(), 0);
  }

  protected cancelSubmit(): void {
    if (this.submitting()) {
      return;
    }
    this.confirmationState.set('idle');
    setTimeout(() => this.submitTrigger?.nativeElement.focus(), 0);
  }

  protected escapeModal(): void {
    if (this.isConfirming() && !this.submitting()) {
      this.cancelSubmit();
    }
  }

  protected async confirmSubmit(): Promise<void> {
    if (this.confirmationState() !== 'confirming' || this.submitting()) {
      return;
    }
    this.confirmationState.set('idle');
    if (!this.isActive()) {
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

  private enqueueMutation(intent: SessionMutation): void {
    if (!this.isActive()) {
      return;
    }
    this.pendingMutations += 1;
    this.saving.set(true);
    this.saveError.set(false);
    this.mutationTail = this.mutationTail
      .then(() => this.executeMutation(intent))
      .catch((error: unknown) => this.handleMutationError(error, intent))
      .finally(() => {
        this.pendingMutations -= 1;
        if (this.pendingMutations === 0) {
          this.saving.set(false);
        }
      });
  }

  private drainMutations(): Promise<void> {
    return this.mutationTail;
  }

  private async executeMutation(intent: SessionMutation): Promise<void> {
    const sessionId = this.sessionId();
    if (sessionId === '' || !this.isActive()) {
      return;
    }
    if (intent.kind === 'save') {
      const updated = await firstValueFrom(
        this.api.saveExamSessionAnswers(sessionId, [
          { examSessionQuestionId: intent.questionId, selectedExamSessionAnswerOptionId: intent.optionId },
        ]),
      );
      this.applySession(updated, { preserveLocalSelection: true, reselectIndex: false });
      if (this.localSelection()[intent.questionId] === intent.optionId) {
        const local = { ...this.localSelection() };
        delete local[intent.questionId];
        this.localSelection.set(local);
      }
      this.announcer.announce(this.i18n.t('session.announceSaved'));
      return;
    }
    if (intent.kind === 'clear') {
      const updated = await firstValueFrom(
        this.api.clearExamSessionAnswer(sessionId, intent.questionId),
      );
      this.applySession(updated, { preserveLocalSelection: true, reselectIndex: false });
      const cleared = { ...this.clearedSelection() };
      delete cleared[intent.questionId];
      this.clearedSelection.set(cleared);
      this.announcer.announce(this.i18n.t('session.announceCleared'));
      return;
    }
    const desired = !this.persistedFlag(intent.questionId);
    const updated = await firstValueFrom(
      this.api.setExamSessionQuestionFlag(sessionId, intent.questionId, desired),
    );
    this.applySession(updated, { preserveLocalSelection: true, reselectIndex: false });
    this.announcer.announce(
      desired ? this.i18n.t('session.announceFlagged') : this.i18n.t('session.announceUnflagged'),
    );
  }

  private async handleMutationError(error: unknown, intent: SessionMutation): Promise<void> {
    if (this.isNotFound(error)) {
      this.notFound.set(true);
      return;
    }
    if (intent.kind === 'clear') {
      const cleared = { ...this.clearedSelection() };
      delete cleared[intent.questionId];
      this.clearedSelection.set(cleared);
    }
    await this.reconcile();
    this.failedIntent = intent;
    this.saveError.set(true);
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
      this.applySession(loaded, { preserveLocalSelection: true, reselectIndex: true });
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

  private applySession(
    loaded: ExamSession,
    options: { preserveLocalSelection: boolean; reselectIndex: boolean },
  ): void {
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
    if (options.reselectIndex) {
      this.selectResumeIndex();
    } else if (loaded.items.length > 0 && this.currentIndex() >= loaded.items.length) {
      this.currentIndex.set(loaded.items.length - 1);
    }
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
      this.applySession(reloaded, { preserveLocalSelection: true, reselectIndex: false });
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
