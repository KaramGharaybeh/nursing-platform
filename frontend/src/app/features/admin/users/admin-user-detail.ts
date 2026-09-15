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
import { LoadingErrorRetry } from '../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../shared/ui/loading-error-retry';

const ROLE_OPTIONS: readonly NpSelectOption[] = Object.freeze([
  { value: 'Nurse', label: 'Nurse' },
  { value: 'Employer', label: 'Employer' },
  { value: 'Expert', label: 'Expert' },
  { value: 'Admin', label: 'Admin' },
]);

@Component({
  selector: 'np-admin-user-detail',
  imports: [LoadingErrorRetry, MatButtonModule, NpSelectControl, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-user-detail.html',
  styleUrl: './admin-user-detail.scss',
})
export class AdminUserDetail implements OnInit {
  private readonly api = inject(AdminUsersApi);
  private readonly route = inject(ActivatedRoute);

  protected readonly usersPath = canonicalRoutePath('ADMIN_USERS');
  protected readonly roleOptions = ROLE_OPTIONS;
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

  protected displayName(user: UserDetailDto): string {
    const name = `${user.firstName} ${user.lastName}`.trim();
    return name === '' ? 'Name not provided' : name;
  }

  protected displayRoles(user: UserDetailDto): string {
    return user.roles.length > 0 ? user.roles.join(', ') : 'No role assigned';
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
      this.saveStatus.set('Role updated successfully.');
    } catch (error: unknown) {
      const normalized = this.normalizeError(error);
      this.saveError.set(normalized.detail.trim() !== '' ? normalized.detail : 'Role could not be updated.');
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
      this.roleControl.setValue(loaded.roles[0] ?? ROLE_OPTIONS[0]?.value ?? '');
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
