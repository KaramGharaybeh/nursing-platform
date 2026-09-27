import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamFullResult } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsReviewPath, canonicalRoutePath } from '../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import { LocalizationService } from '../../core/i18n/localization.service';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

const STATUS_SUBMITTED = 'Submitted';
const STATUS_EXPIRED = 'Expired';

const GENERIC_RETRY_COPY = "We couldn't load this exam result. Try again.";

@Component({
  selector: 'np-exam-result',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './exam-result.html',
  styleUrl: './exam-result.scss',
})
export class ExamResultScreen implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);
  protected readonly i18n = inject(LocalizationService);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly result = signal<ExamFullResult | undefined>(undefined);
  protected readonly unavailable = signal(false);
  protected readonly notFinalized = signal(false);

  protected readonly backPath = canonicalRoutePath('EXAMS_CATALOG');

  protected reviewPath(): string {
    return buildExamsReviewPath(this.examId(), this.result()?.sessionId ?? this.sessionId());
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected statusLabel(): string | undefined {
    const status = this.result()?.status;
    if (status === STATUS_SUBMITTED) {
      return this.i18n.t('result.statusCompleted');
    }
    if (status === STATUS_EXPIRED) {
      return this.i18n.t('result.statusExpired');
    }
    return undefined;
  }

  private sessionId(): string {
    return this.route.snapshot.paramMap.get('sessionId') ?? '';
  }

  private examId(): string {
    return this.route.snapshot.paramMap.get('examId') ?? '';
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.result.set(undefined);
    this.unavailable.set(false);
    this.notFinalized.set(false);
    try {
      const loaded = await firstValueFrom(this.api.getExamSessionResult(this.sessionId()));
      if (loaded.examId !== this.examId()) {
        this.unavailable.set(true);
        this.state.set({ kind: 'ready' });
        return;
      }
      this.result.set(loaded);
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
