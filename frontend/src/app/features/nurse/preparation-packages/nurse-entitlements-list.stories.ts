import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { NurseEntitlementsList } from './nurse-entitlements-list';

const ACTIVE = {
  id: 'ent-active-1',
  packageOfferId: 'offer-1',
  packageOfferTitle: 'NCLEX Preparation Package',
  packageDefinitionId: 'def-1',
  packageDefinitionTitle: 'NCLEX Prep Definition',
  packageVersionId: 'ver-1',
  includedExamId: 'exam-1',
  includedExamTitle: 'NCLEX-RN Readiness Exam',
  accessStartsAt: '2026-09-01T00:00:00Z',
  accessEndsAt: '2026-11-30T00:00:00Z',
  status: 'Active',
  benefitRights: [
    {
      rightType: 'StudyMaterial',
      status: 'Active',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: '2026-11-30T00:00:00Z',
      isAvailable: true,
      isDormant: false,
    },
    {
      rightType: 'Practice',
      status: 'Active',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: null,
      isAvailable: false,
      isDormant: true,
    },
  ],
};

const EXPIRED = {
  id: 'ent-expired-2',
  packageOfferId: 'offer-2',
  packageOfferTitle: 'CGFNS Qualifying Preparation',
  packageDefinitionId: 'def-2',
  packageDefinitionTitle: 'CGFNS Prep Definition',
  packageVersionId: 'ver-2',
  includedExamId: 'exam-2',
  includedExamTitle: 'CGFNS Qualifying Exam',
  accessStartsAt: '2026-01-01T00:00:00Z',
  accessEndsAt: '2026-03-31T00:00:00Z',
  status: 'Expired',
  benefitRights: [],
};

const LONG_TITLE = {
  ...ACTIVE,
  id: 'ent-long-3',
  packageOfferTitle:
    'An exceptionally long preparation package offer title that keeps going so wrapping behavior stays safe on narrow screens',
  packageDefinitionTitle:
    'An exceptionally long preparation package definition title that keeps going so wrapping behavior stays safe',
  includedExamTitle: 'An exceptionally long included examination title that keeps going so wrapping stays safe',
};

function pageOf(items: typeof ACTIVE[], totalCount: number, totalPages: number) {
  return of({
    items,
    page: 1,
    pageSize: 20,
    totalCount,
    totalPages,
  });
}

class PopulatedApi {
  listMyEntitlements() {
    return pageOf([ACTIVE, EXPIRED], 2, 1);
  }
}

class ManyPagesApi extends PopulatedApi {
  override listMyEntitlements() {
    return pageOf([ACTIVE, EXPIRED], 45, 3);
  }
}

class EmptyApi extends PopulatedApi {
  override listMyEntitlements() {
    return pageOf([], 0, 1);
  }
}

class LongTitlesApi extends PopulatedApi {
  override listMyEntitlements() {
    return pageOf([LONG_TITLE], 1, 1);
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: PreparationPackageEntitlementsApi, useValue: api }];
}

const meta: Meta<NurseEntitlementsList> = {
  component: NurseEntitlementsList,
  title: 'Features/Nurse/PreparationPackages/EntitlementsList',
};
export default meta;
type Story = StoryObj<NurseEntitlementsList>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const MultiplePages: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ManyPagesApi()) })],
};

export const LongTitles: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LongTitlesApi()) })],
};
