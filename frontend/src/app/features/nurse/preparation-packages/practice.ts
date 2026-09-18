import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PreparationPackagePracticeApi } from '../../../core/api/preparation-package-practice-api';
import type { PackagePracticeAnswerSubmissionDto } from '../../../core/api/generated/models/package-practice-answer-submission-dto';
import type { PackagePracticeContentListDto } from '../../../core/api/generated/models/package-practice-content-list-dto';
import type { PackagePracticeItemContentDto } from '../../../core/api/generated/models/package-practice-item-content-dto';
import type { PackagePracticeProgressSummaryDto } from '../../../core/api/generated/models/package-practice-progress-summary-dto';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { buildCanonicalRoutePath } from '../../../core/routing/canonical-routes';
import { Announcer, NpLiveRegion } from '../../../shared/ui/announcement';
import { NpEmptyState } from '../../../shared/ui/empty-state';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

const STATE_UNANSWERED = 0;
const STATE_ANSWERED_CORRECT = 1;

@Component({
  selector: 'np-practice',
  imports: [LoadingErrorRetry, NpEmptyState, NpLiveRegion, RouterLink],
  templateUrl: './practice.html',
  styleUrl: './practice.scss',
})
export class Practice implements OnInit {
  private readonly api = inject(PreparationPackagePracticeApi);
  private readonly route = inject(ActivatedRoute);
  private readonly announcer = inject(Announcer);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly notFound = signal(false);
  protected readonly accessEnded = signal(false);
  protected readonly content = signal<PackagePracticeContentListDto | undefined>(undefined);
  protected readonly progress = signal<PackagePracticeProgressSummaryDto | undefined>(undefined);
  protected readonly currentIndex = signal(0);
  protected readonly selectedOptionId = signal<string | undefined>(undefined);
  protected readonly submitting = signal(false);
  protected readonly feedbackByItem = signal<Readonly<Record<string, string>>>({});

  protected backPath = '';

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected currentItem(): PackagePracticeItemContentDto | undefined {
    return this.content()?.items[this.currentIndex()];
  }

  protected currentItemState(): number {
    const item = this.currentItem();
    const progress = this.progress();
    if (item === undefined || progress === undefined) {
      return STATE_UNANSWERED;
    }
    return progress.itemStates.find((entry) => entry.practiceItemId === item.practiceItemId)?.state
      ?? STATE_UNANSWERED;
  }

  protected isAnswered(): boolean {
    return this.currentItemState() !== STATE_UNANSWERED;
  }

  protected isCorrect(): boolean {
    return this.currentItemState() === STATE_ANSWERED_CORRECT;
  }

  protected feedback(): string | undefined {
    const item = this.currentItem();
    if (item === undefined || !this.isAnswered()) {
      return undefined;
    }
    return this.feedbackByItem()[item.practiceItemId];
  }

  protected isComplete(): boolean {
    const progress = this.progress();
    return (
      progress !== undefined && progress.totalItems > 0 && progress.answeredCount >= progress.totalItems
    );
  }

  protected isEmpty(): boolean {
    return (this.content()?.items.length ?? 0) === 0;
  }

  protected progressSummary(): string {
    const progress = this.progress();
    if (progress === undefined) {
      return '';
    }
    return `${progress.answeredCount} of ${progress.totalItems} answered`;
  }

  protected selectOption(optionId: string): void {
    this.selectedOptionId.set(optionId);
  }

  protected async submit(): Promise<void> {
    const item = this.currentItem();
    const selected = this.selectedOptionId();
    const entitlementId = this.entitlementId();
    if (item === undefined || selected === undefined || this.submitting()) {
      return;
    }
    this.submitting.set(true);
    try {
      const result = await firstValueFrom(
        this.api.submitAnswer(entitlementId, item.practiceItemId, selected),
      );
      this.feedbackByItem.set({ ...this.feedbackByItem(), [item.practiceItemId]: result.immediateFeedback });
      this.applySubmissionResult(result);
      await this.refreshProgress();
      this.announcer.announce(result.state === STATE_ANSWERED_CORRECT ? 'Correct.' : 'Incorrect.');
    } catch (error: unknown) {
      if (this.isConflict(error)) {
        this.accessEnded.set(true);
      } else {
        this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
      }
    } finally {
      this.submitting.set(false);
    }
  }

  protected previous(): void {
    this.goTo(this.currentIndex() - 1);
  }

  protected next(): void {
    this.goTo(this.currentIndex() + 1);
  }

  protected goTo(index: number): void {
    const total = this.content()?.items.length ?? 0;
    if (total === 0) {
      return;
    }
    const clamped = Math.min(Math.max(index, 0), total - 1);
    this.currentIndex.set(clamped);
    this.restoreSelection();
  }

  private entitlementId(): string {
    return this.route.snapshot.paramMap.get('entitlementId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    this.accessEnded.set(false);
    this.backPath = buildCanonicalRoutePath('PREPARATION_PACKAGES_ENTITLEMENT_DETAIL', {
      entitlementId: this.entitlementId(),
    });

    const [itemsResult, progressResult] = await Promise.allSettled([
      firstValueFrom(this.api.getItems(this.entitlementId())),
      firstValueFrom(this.api.getProgress(this.entitlementId())),
    ]);

    if (itemsResult.status === 'fulfilled') {
      this.content.set(itemsResult.value);
    } else if (this.isNotFound(itemsResult.reason)) {
      this.notFound.set(true);
    } else if (this.isConflict(itemsResult.reason)) {
      this.accessEnded.set(true);
    } else {
      this.state.set({ kind: 'error', error: this.normalizeError(itemsResult.reason), canRetry: true });
      return;
    }

    if (progressResult.status === 'fulfilled') {
      this.progress.set(progressResult.value);
    } else if (this.isNotFound(progressResult.reason)) {
      this.notFound.set(true);
    } else if (!this.accessEnded()) {
      this.state.set({ kind: 'error', error: this.normalizeError(progressResult.reason), canRetry: true });
      return;
    }

    this.state.set({ kind: 'ready' });
    this.selectResumeIndex();
  }

  private applySubmissionResult(result: PackagePracticeAnswerSubmissionDto): void {
    const current = this.progress();
    if (current === undefined) {
      return;
    }
    this.progress.set({
      ...current,
      itemStates: current.itemStates.map((entry) =>
        entry.practiceItemId === result.practiceItemId
          ? {
              ...entry,
              state: result.state,
              selectedPracticeAnswerOptionId: result.selectedPracticeAnswerOptionId,
              lastAnsweredAt: result.lastAnsweredAt,
            }
          : entry,
      ),
    });
  }

  private async refreshProgress(): Promise<void> {
    try {
      const refreshed = await firstValueFrom(this.api.getProgress(this.entitlementId()));
      this.progress.set(refreshed);
      this.restoreSelection();
    } catch (error: unknown) {
      if (this.isConflict(error)) {
        this.accessEnded.set(true);
      } else if (this.isNotFound(error)) {
        this.notFound.set(true);
      } else {
        this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
      }
    }
  }

  private selectResumeIndex(): void {
    const items = this.content()?.items ?? [];
    const progress = this.progress();
    if (items.length === 0) {
      this.currentIndex.set(0);
      return;
    }
    const firstUnanswered = items.findIndex((item) =>
      (progress?.itemStates.find((entry) => entry.practiceItemId === item.practiceItemId)?.state
        ?? STATE_UNANSWERED) === STATE_UNANSWERED,
    );
    this.currentIndex.set(firstUnanswered === -1 ? 0 : firstUnanswered);
    this.restoreSelection();
  }

  private restoreSelection(): void {
    const item = this.currentItem();
    const progress = this.progress();
    if (item === undefined || progress === undefined) {
      this.selectedOptionId.set(undefined);
      return;
    }
    this.selectedOptionId.set(
      progress.itemStates.find((entry) => entry.practiceItemId === item.practiceItemId)
        ?.selectedPracticeAnswerOptionId ?? undefined,
    );
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
