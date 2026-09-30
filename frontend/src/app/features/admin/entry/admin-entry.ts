import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import type { TranslationKey } from '../../../core/i18n/translations';
import { LocalizationService } from '../../../core/i18n/localization.service';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import type { CanonicalRouteId } from '../../../core/routing/canonical-routes';
import { filterEligibleNavigationCandidates } from '../../../core/routing/navigation-permission-policy';
import type { NavigationUserState } from '../../../core/routing/navigation-permission-policy';

interface AdminWorkspaceDestination {
  readonly routeId: CanonicalRouteId;
  readonly titleKey: TranslationKey;
  readonly descriptionKey: TranslationKey;
  readonly groupKey: TranslationKey;
}

const ADMIN_WORKSPACE_DESTINATIONS: readonly AdminWorkspaceDestination[] = Object.freeze([
  {
    routeId: 'ADMIN_USERS',
    titleKey: 'adm.destUsersTitle',
    descriptionKey: 'adm.destUsersDesc',
    groupKey: 'adm.destUsersGroup',
  },
  {
    routeId: 'ADMIN_REFERENCE_DATA',
    titleKey: 'adm.destCatTitle',
    descriptionKey: 'adm.destCatDesc',
    groupKey: 'adm.destExamsGroup',
  },
  {
    routeId: 'ADMIN_EXAMS',
    titleKey: 'adm.destExamsTitle',
    descriptionKey: 'adm.destExamsDesc',
    groupKey: 'adm.destExamsGroup',
  },
]);

@Component({
  selector: 'np-admin-entry',
  imports: [MatButtonModule, RouterLink],
  templateUrl: './admin-entry.html',
  styleUrl: './admin-entry.scss',
})
export class AdminEntry {
  private readonly currentUserStore = inject(CurrentUserStore);
  protected readonly i18n = inject(LocalizationService);

  protected t(key: TranslationKey): string {
    return this.i18n.t(key);
  }

  protected readonly destinations = computed(() => {
    const eligibility = filterEligibleNavigationCandidates(
      ADMIN_WORKSPACE_DESTINATIONS.map((destination) => destination.routeId),
      this.userState(),
    );
    if (eligibility.status !== 'resolved') {
      return [];
    }
    return ADMIN_WORKSPACE_DESTINATIONS.filter((destination) =>
      eligibility.eligible.includes(destination.routeId),
    );
  });

  protected destinationPath(routeId: CanonicalRouteId): string {
    return canonicalRoutePath(routeId);
  }

  private userState(): NavigationUserState {
    const status = this.currentUserStore.status();
    if (status !== 'ready') {
      return { status };
    }
    const currentUser = this.currentUserStore.currentUser();
    return {
      status: 'ready',
      roles: currentUser?.roles ?? [],
      permissions: currentUser?.permissions ?? [],
    };
  }
}
