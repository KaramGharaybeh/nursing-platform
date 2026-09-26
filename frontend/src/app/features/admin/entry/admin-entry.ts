import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';
import type { CanonicalRouteId } from '../../../core/routing/canonical-routes';
import { filterEligibleNavigationCandidates } from '../../../core/routing/navigation-permission-policy';
import type { NavigationUserState } from '../../../core/routing/navigation-permission-policy';

interface AdminWorkspaceDestination {
  readonly routeId: CanonicalRouteId;
  readonly title: string;
  readonly description: string;
  readonly group: string;
}

const ADMIN_WORKSPACE_DESTINATIONS: readonly AdminWorkspaceDestination[] = Object.freeze([
  {
    routeId: 'ADMIN_USERS',
    title: 'Users',
    description: 'Find user accounts and open safe profile details.',
    group: 'Access',
  },
  {
    routeId: 'ADMIN_REFERENCE_DATA',
    title: 'Exam categories',
    description: 'Manage exam categories for supported countries.',
    group: 'Exams',
  },
  {
    routeId: 'ADMIN_EXAMS',
    title: 'Manage exams',
    description: 'View and manage exam records.',
    group: 'Exams',
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
