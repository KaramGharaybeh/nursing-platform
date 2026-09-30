import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AdminUsersApi } from '../../../core/api/admin-users-api';
import type { UserDetailDto } from '../../../core/api/generated/models/user-detail-dto';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { normalizeProblemDetails } from '../../../core/api/problem-details';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import { NpSelectControl } from '../../../shared/ui/form-controls';
import type { NpSelectOption } from '../../../shared/ui/form-controls';
import type { TranslationKey } from '../../../core/i18n/translations';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

const ROLE_VALUES = Object.freeze(['Nurse', 'Employer', 'Expert', 'Admin'] as const);

@Component({
  selector: 'np-admin-user-detail',
  imports: [LoadingErrorRetry, MatButtonModule, NpSelectControl, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-user-detail.html',
  styleUrl: './admin-user-detail.scss',
})
export class AdminUserDetail implements OnInit {
  private readonly api = inject(AdminUsersApi);
  private readonly route = inject(ActivatedRoute);
  protected readonly i18n = inject(LocalizationService);

  protected readonly usersPath = canonicalRoutePath('ADMIN_USERS');
  protected get roleOptions(): readonly NpSelectOption[] {
    return [
      { value: 'Nurse', label: this.i18n.t('adm.roleNurse') },
      { value: 'Employer', label: this.i18n.t('adm.roleEmployer') },
      { value: 'Expert', label: this.i18n.t('adm.roleExpert') },
      { value: 'Admin', label: this.i18n.t('adm.roleAdmin') },
    ];
  }
  protected readonly roleControl = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  protected readonly user = signal<UserDetailDto | undefined>(undefined);
  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly savePending = signal(false);
  protected readonly saveStatus = signal('');
  protected readonly saveError = signal('');

  ngOnInit(): void {
    void this.load();
  }

  protected get roleValue(): string {
    return this.roleControl.value;
  }

  updateRole(value: string): void {
    this.roleControl.setValue(value);
    this.saveStatus.set('');
    this.saveError.set('');
  }

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected displayName(user: UserDetailDto): string {
    const name = `${user.firstName} ${user.lastName}`.trim();
    return name === '' ? this.i18n.t('adm.noName') : name;
  }

  protected displayRoles(user: UserDetailDto): string {
    return user.roles.length > 0 ? user.roles.join(', ') : this.i18n.t('adm.noRole');
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  async saveRole(): Promise<void> {
    const current = this.user();
    if (current === undefined || this.roleControl.invalid) {
      this.roleControl.markAsTouched();
      return;
    }

    this.savePending.set(true);
    this.saveStatus.set('');
    this.saveError.set('');
    try {
      const response = await firstValueFrom(this.api.updateRole(current.id, { roleName: this.roleValue }));
      this.user.set({ ...current, roles: response.roles });
      this.roleControl.setValue(response.roles[0] ?? this.roleValue);
      this.saveStatus.set(this.i18n.t('adm.roleUpdated'));
    } catch (error: unknown) {
      const normalized = this.normalizeError(error);
      this.saveError.set(this.i18n.backendErrorCopy(normalized.detail, 'adm.roleUpdateFailed'));
    } finally {
      this.savePending.set(false);
    }
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    const userId = this.route.snapshot.paramMap.get('userId') ?? '';
    try {
      const loaded = await firstValueFrom(this.api.get(userId));
      this.user.set(loaded);
      this.roleControl.setValue(loaded.roles[0] ?? ROLE_VALUES[0] ?? '');
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
