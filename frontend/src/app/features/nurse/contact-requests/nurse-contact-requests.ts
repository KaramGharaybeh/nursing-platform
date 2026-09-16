import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom, type Observable } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../shared/ui/announcement';
import { ContactRequestsApi } from '../../../core/api/contact-requests-api';
import type { ContactRequestStatus } from '../../../core/api/generated/models/contact-request-status';
import type { PaginatedResultOfReceivedContactRequestDto } from '../../../core/api/generated/models/paginated-result-of-received-contact-request-dto';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { NpEmptyState } from '../../../shared/ui/empty-state';
import { NpSelectControl, type NpSelectOption } from '../../../shared/ui/form-controls';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';
import { NpPagination, resolveListState } from '../../../shared/ui/pagination';
import { NurseContactRequestCard } from './nurse-contact-request-card';

const PAGE_SIZE = 20;

const STATUS_OPTIONS: readonly { value: string; label: string; status?: ContactRequestStatus }[] = [
  { value: '', label: 'All' },
  { value: '0', label: 'Pending', status: 0 },
  { value: '1', label: 'Approved', status: 1 },
  { value: '2', label: 'Rejected', status: 2 },
  { value: '3', label: 'Cancelled', status: 3 },
];

@Component({
  selector: 'np-nurse-contact-requests',
  imports: [LoadingErrorRetry, MatButtonModule, NpEmptyState, NpLiveRegion, NpPagination, NpSelectControl, NurseContactRequestCard],
  templateUrl: './nurse-contact-requests.html',
  styleUrl: './nurse-contact-requests.scss',
})
export class NurseContactRequests implements OnInit {
  private readonly api = inject(ContactRequestsApi);
  private readonly announcer = inject(Announcer);

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly result = signal<PaginatedResultOfReceivedContactRequestDto | undefined>(undefined);
  protected readonly statusFilter = signal<ContactRequestStatus | undefined>(undefined);
  protected readonly page = signal(1);
  protected readonly notice = signal('');
  protected readonly mutatingIds = signal<readonly string[]>([]);
  protected readonly cardErrors = signal<Readonly<Record<string, string>>>({});

  protected readonly pageSize = PAGE_SIZE;
  protected readonly statusOptions: readonly NpSelectOption[] = STATUS_OPTIONS.map(({ value, label }) => ({
    value,
    label,
  }));

  protected get statusValue(): string {
    const current = this.statusFilter();
    return STATUS_OPTIONS.find((option) => option.status === current)?.value ?? '';
  }

  protected get statusLabel(): string {
    const current = this.statusFilter();
    return STATUS_OPTIONS.find((option) => option.status === current)?.label.toLowerCase() ?? '';
  }

  protected get hasActiveFilter(): boolean {
    return this.statusFilter() !== undefined;
  }

  protected get isEmptyResult(): boolean {
    const current = this.result();
    return current !== undefined && current.items.length === 0 && current.totalCount === 0;
  }

  ngOnInit(): void {
    void this.load(1);
  }

  protected async retry(): Promise<void> {
    await this.load(this.page());
  }

  protected async loadPage(page: number): Promise<void> {
    await this.load(page);
  }

  protected async updateStatusFilter(value: string): Promise<void> {
    const selected = STATUS_OPTIONS.find((option) => option.value === value);
    this.statusFilter.set(selected?.status);
    await this.load(1);
  }

  protected async clearFilter(): Promise<void> {
    this.statusFilter.set(undefined);
    await this.load(1);
  }

  protected async refreshAfterMutation(): Promise<void> {
    await this.load(this.page());
    const current = this.result();
    if (current !== undefined && current.items.length === 0 && current.totalCount > 0) {
      await this.load(Math.max(1, current.totalPages));
    }
  }

  protected async approve(id: string): Promise<void> {
    await this.mutate(id, () => this.api.approve(id), 'Contact request approved.');
  }

  protected async reject(id: string): Promise<void> {
    await this.mutate(id, () => this.api.reject(id), 'Contact request rejected.');
  }

  protected isMutating(id: string): boolean {
    return this.mutatingIds().includes(id);
  }

  protected cardError(id: string): string {
    return this.cardErrors()[id] ?? '';
  }

  private async mutate(
    id: string,
    action: () => Observable<unknown>,
    successAnnouncement: string,
  ): Promise<void> {
    this.mutatingIds.set([...this.mutatingIds(), id]);
    this.cardErrors.set({ ...this.cardErrors(), [id]: '' });
    try {
      await firstValueFrom(action());
      this.announcer.announce(successAnnouncement);
      await this.refreshAfterMutation();
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.notice.set('That request is no longer available. The list has been refreshed.');
        this.announcer.announce('This request was already decided.');
        await this.refreshAfterMutation();
      } else if (this.isConflict(error)) {
        this.notice.set('This request was already decided. The list has been refreshed.');
        this.announcer.announce('This request was already decided.');
        await this.refreshAfterMutation();
      } else {
        const normalized = normalizeProblemDetails(this.errorBody(error));
        const message =
          normalized.detail.trim() !== '' ? normalized.detail : 'This action could not be completed. Try again.';
        this.cardErrors.set({ ...this.cardErrors(), [id]: message });
      }
    } finally {
      this.mutatingIds.set(this.mutatingIds().filter((entry) => entry !== id));
    }
  }

  private async load(page: number): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const status = this.statusFilter();
      const result = await firstValueFrom(
        this.api.listReceived({ page, pageSize: PAGE_SIZE, status }),
      );
      this.result.set(result);
      this.page.set(result.page);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
    }
  }

  protected listState(): 'results' | 'empty' | 'no-results' {
    const current = this.result();
    return resolveListState((current?.items.length ?? 0) > 0, this.hasActiveFilter);
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 404;
  }

  private isConflict(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 409;
  }

  private errorBody(error: unknown): unknown {
    if (typeof error !== 'object' || error === null) {
      return error;
    }
    if ('error' in error) {
      return (error as { error?: unknown }).error;
    }
    return error;
  }
}
