import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AdminUsersApi } from '../../../core/api/admin-users-api';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { UserListItemDto } from '../../../core/api/generated/models/user-list-item-dto';
import { buildAdminUserDetailPath } from '../../../core/routing/canonical-routes';
import type { TranslationKey } from '../../../core/i18n/translations';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { NpTextInputControl } from '../../../shared/ui/form-controls';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

@Component({
  selector: 'np-admin-users',
  imports: [LoadingErrorRetry, MatButtonModule, NpTextInputControl, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.scss',
})
export class AdminUsers implements OnInit {
  private readonly api = inject(AdminUsersApi);
  protected readonly i18n = inject(LocalizationService);

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly users = signal<readonly UserListItemDto[]>([]);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly totalCount = signal(0);
  protected readonly totalPages = signal(0);
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });

  ngOnInit(): void {
    void this.load(1);
  }

  protected get searchValue(): string {
    return this.searchControl.value;
  }

  updateSearch(value: string): void {
    this.searchControl.setValue(value);
  }

  async submitSearch(): Promise<void> {
    await this.load(1);
  }

  protected async nextPage(): Promise<void> {
    if (this.page() < this.totalPages()) {
      await this.load(this.page() + 1);
    }
  }

  protected async previousPage(): Promise<void> {
    if (this.page() > 1) {
      await this.load(this.page() - 1);
    }
  }

  protected detailPath(userId: string): string {
    return buildAdminUserDetailPath(userId);
  }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected tp(key: TranslationKey, params: Record<string, string | number>): string {
    return this.i18n.tp(key, params);
  }

  protected displayName(user: UserListItemDto): string {
    const name = `${user.firstName} ${user.lastName}`.trim();
    return name === '' ? this.i18n.t('adm.noName') : name;
  }

  protected displayRoles(user: UserListItemDto): string {
    return user.roles.length > 0 ? user.roles.join(', ') : this.i18n.t('adm.noRole');
  }

  protected async retry(): Promise<void> {
    await this.load(this.page());
  }

  private async load(page: number): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      const result = await firstValueFrom(this.api.list({
        page,
        pageSize: this.pageSize(),
        search: this.searchValue.trim() === '' ? undefined : this.searchValue.trim(),
        sort: 'name',
      }));
      this.users.set(result.items);
      this.page.set(result.page);
      this.pageSize.set(result.pageSize);
      this.totalCount.set(result.totalCount);
      this.totalPages.set(result.totalPages);
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      this.state.set({ kind: 'error', error: this.normalizeError(error), canRetry: true });
    }
  }

  private normalizeError(error: unknown): NormalizedProblemDetails {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      return normalizeProblemDetails((error as { error?: unknown }).error);
    }
    return normalizeProblemDetails(error);
  }
}
