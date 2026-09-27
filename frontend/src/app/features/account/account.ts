import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../shared/ui/announcement';
import { CurrentUserStore } from '../../core/auth/current-user-store';
import type { CurrentUser } from '../../core/auth/current-user';
import { ProfileApi } from '../../core/api/profile-api';
import { normalizeProblemDetails } from '../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { LocalizationService } from '../../core/i18n/localization.service';
import { LoadingErrorRetry } from '../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../shared/ui/loading-error-retry';
import { PersonalDetailsForm, type PersonalDetailsFormValue } from './personal-details-form';

@Component({
  selector: 'np-account',
  imports: [LoadingErrorRetry, MatButtonModule, NpLiveRegion, PersonalDetailsForm],
  templateUrl: './account.html',
  styleUrl: './account.scss',
})
export class Account {
  private readonly currentUserStore = inject(CurrentUserStore);
  private readonly profileApi = inject(ProfileApi);
  private readonly announcer = inject(Announcer);
  protected readonly i18n = inject(LocalizationService);

  protected readonly isEditing = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly saveError = signal<NormalizedProblemDetails | undefined>(undefined);

  protected get user(): CurrentUser | undefined {
    return this.currentUserStore.currentUser();
  }

  protected get state(): LoadingErrorRetryState {
    if (this.user === undefined) {
      return { kind: 'loading' };
    }
    return { kind: 'ready' };
  }

  protected get displayName(): string {
    const user = this.user;
    if (user === undefined) {
      return '';
    }
    const name = `${user.firstName} ${user.lastName}`.trim();
    return name === '' ? user.username : name;
  }

  protected get verificationLabel(): string {
    return this.user?.emailVerified === true
      ? this.i18n.t('account.verified')
      : this.i18n.t('account.notVerified');
  }

  protected get formInitial(): PersonalDetailsFormValue {
    const user = this.user;
    return { firstName: user?.firstName ?? '', lastName: user?.lastName ?? '' };
  }

  protected startEdit(): void {
    this.saveError.set(undefined);
    this.isEditing.set(true);
  }

  protected cancelEdit(): void {
    this.saveError.set(undefined);
    this.isEditing.set(false);
  }

  protected async submitEdit(value: PersonalDetailsFormValue): Promise<void> {
    if (this.isSubmitting()) {
      return;
    }
    this.isSubmitting.set(true);
    this.saveError.set(undefined);
    try {
      await firstValueFrom(
        this.profileApi.updateCurrentUserProfile({
          firstName: value.firstName,
          lastName: value.lastName,
        }),
      );
      await firstValueFrom(this.currentUserStore.hydrate());
      this.isEditing.set(false);
      this.announcer.announce(this.i18n.t('account.saved'));
    } catch (error: unknown) {
      this.saveError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isSubmitting.set(false);
    }
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
