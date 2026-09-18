import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamDetail as ExamDetailModel } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { buildExamsInstructionsPath, canonicalRoutePath } from '../../core/routing/canonical-routes';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';

@Component({
  selector: 'np-exam-detail',
  imports: [LoadingErrorRetry, RouterLink],
  templateUrl: './exam-detail.html',
  styleUrl: './exam-detail.scss',
})
export class ExamDetail implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);

  protected readonly backPath = canonicalRoutePath('EXAMS_CATALOG');
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly exam = signal<ExamDetailModel | undefined>(undefined);
  protected readonly notFound = signal(false);

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected hasDescription(): boolean {
    return (this.exam()?.description?.trim() ?? '') !== '';
  }

  protected hasInstructions(): boolean {
    return (this.exam()?.instructions?.trim() ?? '') !== '';
  }

  protected hasCountry(): boolean {
    return this.exam()?.countryName.trim() !== '';
  }

  protected hasCategory(): boolean {
    return (this.exam()?.categoryName?.trim() ?? '') !== '';
  }

  protected isStartable(): boolean {
    return this.exam()?.canStart === true;
  }

  protected requiresPurchase(): boolean {
    const exam = this.exam();
    return exam !== undefined && exam.isFree === false && exam.canStart === false;
  }

  protected instructionsPath(): string {
    return buildExamsInstructionsPath(this.route.snapshot.paramMap.get('examId') ?? '');
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    this.notFound.set(false);
    const examId = this.route.snapshot.paramMap.get('examId') ?? '';
    try {
      const loaded = await firstValueFrom(this.api.getExam(examId));
      this.exam.set(loaded);
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
