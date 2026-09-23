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
    description: 'Maintain the approved category reference data for exams.',
    group: 'Exam setup',
  },
  {
    routeId: 'ADMIN_EXAMS',
    title: 'Exams',
    description: 'Open the exam administration workspace.',
    group: 'Exam setup',
  },
  {
    routeId: 'ADMIN_EXAM_QUESTIONS',
    title: 'Questions',
    description: 'Use the approved question and answer-option workspace.',
    group: 'Exam setup',
  },
  {
    routeId: 'ADMIN_PAYMENT_PRODUCTS',
    title: 'Payment products',
    description: 'Maintain exam-access product setup without order reporting.',
    group: 'Commerce setup',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_TOPICS',
    title: 'Reporting topics',
    description: 'Manage preparation-package reporting topic taxonomy.',
    group: 'Preparation packages',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_PROFILES',
    title: 'Reporting profiles',
    description: 'Manage approved reporting profile publications.',
    group: 'Preparation packages',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_MATERIALS',
    title: 'Study materials',
    description: 'Maintain approved study material records and versions.',
    group: 'Preparation packages',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_PRACTICE_COLLECTIONS',
    title: 'Practice collections',
    description: 'Manage practice collections, items, and answer options.',
    group: 'Preparation packages',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_DEFINITIONS',
    title: 'Package definitions',
    description: 'Maintain approved package definitions and versions.',
    group: 'Preparation packages',
  },
  {
    routeId: 'ADMIN_PREPARATION_PACKAGE_OFFERS',
    title: 'Package offers',
    description: 'Manage approved preparation-package offers.',
    group: 'Preparation packages',
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
