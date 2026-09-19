import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamReview, ExamReviewQuestion } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsResultPath } from '../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

const STATUS_EXPIRED = 'Expired';

const GENERIC_RETRY_COPY = "We couldn't load this exam review. Try again.";

@Component({
  selector: 'np-exam-review',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './exam-review.html',
  styleUrl: './exam-review.scss',
})
export class ExamReviewScreen implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly review = signal<ExamReview | undefined>(undefined);
  protected readonly currentIndex = signal(0);
  protected readonly unavailable = signal(false);
  protected readonly notFinalized = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected backPath(): string {
    return buildExamsResultPath(this.examId(), this.sessionId());
  }

  protected currentQuestion(): ExamReviewQuestion | undefined {
    return this.review()?.items[this.currentIndex()];
  }

  protected questionStatus(question: ExamReviewQuestion): 'Correct' | 'Incorrect' | 'Unanswered' {
    const selected = question.options.find((option) => option.isSelected);
    if (selected === undefined) {
      return 'Unanswered';
    }
    return selected.isCorrect ? 'Correct' : 'Incorrect';
  }

  protected isExpired(): boolean {
    return this.review()?.status === STATUS_EXPIRED;
  }

  protected previous(): void {
    this.goTo(this.currentIndex() - 1);
  }

  protected next(): void {
    this.goTo(this.currentIndex() + 1);
  }

  protected goTo(index: number): void {
    const total = this.review()?.items.length ?? 0;
    if (total === 0) {
      return;
    }
    this.currentIndex.set(Math.min(Math.max(index, 0), total - 1));
  }

  private sessionId(): string {
    return this.route.snapshot.paramMap.get('sessionId') ?? '';
  }

  private examId(): string {
    return this.route.snapshot.paramMap.get('examId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.review.set(undefined);
    this.currentIndex.set(0);
    this.unavailable.set(false);
    this.notFinalized.set(false);
    try {
      const loaded = await firstValueFrom(this.api.getExamSessionReview(this.sessionId()));
      if (loaded.examId !== this.examId()) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
        return;
      }
      this.review.set(loaded);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
      } else if (this.isConflict(error)) {
        this.notFinalized.set(true);
        this.state.set({ kind: 'ready' });
      } else {
        this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
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

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: GENERIC_RETRY_COPY, detail: '' };
  }
}
