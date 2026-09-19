import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamHistoryPage } from '../../core/api/exams-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import {
  buildExamsResultPath,
  buildExamsReviewPath,
  buildExamsSessionPath,
  canonicalRoutePath,
} from '../../core/routing/canonical-routes';
import { NpEmptyState } from '../../shared/ui/empty-state';
import { NpSelectControl } from '../../shared/ui/form-controls';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { NpPagination } from '../../shared/ui/pagination';

export type ExamHistoryStatusFilter = '' | 'InProgress' | 'Submitted' | 'Expired';

const STATUS_LABELS: Record<Exclude<ExamHistoryStatusFilter, ''>, string> = {
  InProgress: 'In progress',
  Submitted: 'Completed',
  Expired: 'Time expired',
};

const STATUS_NUMBERS: Record<Exclude<ExamHistoryStatusFilter, ''>, number> = {
  InProgress: 0,
  Submitted: 1,
  Expired: 2,
};

const GENERIC_RETRY_COPY = "We couldn't load your exam history. Try again.";

@Component({
  selector: 'np-exam-history',
  imports: [DatePipe, LoadingErrorRetry, NpEmptyState, NpPagination, NpSelectControl, RouterLink],
  templateUrl: './exam-history.html',
  styleUrl: './exam-history.scss',
})
export class ExamHistoryScreen implements OnInit {
  private readonly api = inject(ExamsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly page = signal<ExamHistoryPage | undefined>(undefined);
  protected readonly filter = signal<ExamHistoryStatusFilter>('');
  protected readonly pageNumber = signal(1);
  protected readonly hasLoadedOnce = signal(false);

  protected readonly backPath = canonicalRoutePath('EXAMS_CATALOG');

  ngOnInit(): void {
    void this.initialize();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected applyStatusFilter(value: string): void {
    this.filter.set(normalizeStatusFilter(value));
    this.pageNumber.set(1);
    void this.router.navigate([], { queryParams: toQueryParams(this.filter(), 1) });
    void this.load();
  }

  protected clearFilter(): void {
    this.filter.set('');
    this.pageNumber.set(1);
    void this.router.navigate([], { queryParams: {} });
    void this.load();
  }

  protected async loadHistoryPage(page: number): Promise<void> {
    this.pageNumber.set(page);
    void this.router.navigate([], {
      queryParams: toQueryParams(this.filter(), page),
    });
    await this.load();
  }

  protected statusLabel(status: string): string {
    if (status === 'Abandoned') {
      return 'Abandoned';
    }
    return STATUS_LABELS[status as Exclude<ExamHistoryStatusFilter, ''>] ?? status;
  }

  protected isFinalizedStatus(status: string): boolean {
    return status === 'Submitted' || status === 'Expired';
  }

  protected resumePath(examId: string, sessionId: string): string {
    return buildExamsSessionPath(examId, sessionId);
  }

  protected resultPath(examId: string, sessionId: string): string {
    return buildExamsResultPath(examId, sessionId);
  }

  protected reviewPath(examId: string, sessionId: string): string {
    return buildExamsReviewPath(examId, sessionId);
  }

  protected statusOptions(): { value: string; label: string }[] {
    return [
      { value: '', label: 'All' },
      { value: 'InProgress', label: 'In progress' },
      { value: 'Submitted', label: 'Completed' },
      { value: 'Expired', label: 'Time expired' },
    ];
  }

  private async initialize(): Promise<void> {
    const query = this.route.snapshot.queryParamMap;
    this.filter.set(normalizeStatusFilter(query.get('status')));
    this.pageNumber.set(normalizePage(query.get('page')));
    await this.load();
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const filter = this.filter();
      const result = await firstValueFrom(
        this.api.listExamHistory(
          filter === '' ? {} : { status: STATUS_NUMBERS[filter] },
          this.pageNumber(),
        ),
      );
      this.page.set(result);
      this.pageNumber.set(result.page);
      this.hasLoadedOnce.set(true);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.retryableError(error), canRetry: true });
    }
  }

  private retryableError(error: unknown): NormalizedProblemDetails {
    const normalized =
      typeof error === 'object' && error !== null && 'error' in error
        ? normalizeProblemDetails((error as { error?: unknown }).error)
        : normalizeProblemDetails(error);
    return { ...normalized, title: GENERIC_RETRY_COPY, detail: '' };
  }
}

function normalizeStatusFilter(value: string | null): ExamHistoryStatusFilter {
  if (value === 'InProgress' || value === 'Submitted' || value === 'Expired') {
    return value;
  }
  return '';
}

function normalizePage(value: string | null): number {
  const parsed = value === null ? Number.NaN : Number.parseInt(value, 10);
  return Number.isInteger(parsed) && parsed >= 1 ? parsed : 1;
}

function toQueryParams(filter: ExamHistoryStatusFilter, page: number): Record<string, string> {
  return {
    ...(filter === '' ? {} : { status: filter }),
    page: String(page),
  };
}
