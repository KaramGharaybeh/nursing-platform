import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { NurseEntitlementDetail } from './nurse-entitlement-detail';

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
  ],
  purchasedSnapshot: {
    packageOfferSummary: 'Focused preparation for first-time test takers.',
  },
};

const EXPIRED = {
  ...ACTIVE,
  id: 'ent-expired-2',
  status: 'Expired',
  accessStartsAt: '2026-01-01T00:00:00Z',
  accessEndsAt: '2026-03-31T00:00:00Z',
  purchasedSnapshot: {},
};

const MULTIPLE_RIGHTS = {
  ...ACTIVE,
  benefitRights: [
    ...(ACTIVE.benefitRights as unknown[]),
    {
      rightType: 'Practice',
      status: 'Active',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: null,
      isAvailable: false,
      isDormant: true,
    },
    {
      rightType: 'ExamAttempt',
      status: 'Locked',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: null,
      isAvailable: false,
      isDormant: false,
    },
  ],
};

class DetailApi {
  constructor(private readonly detail: typeof ACTIVE) {}

  getMyEntitlement() {
    return of(this.detail);
  }
}

class MissingApi extends DetailApi {
  constructor() {
    super(ACTIVE);
  }

  override getMyEntitlement() {
    return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
  }
}

function providers(api: DetailApi | MissingApi) {
  return [
    provideRouter([]),
    { provide: PreparationPackageEntitlementsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ entitlementId: 'ent-active-1' }) } },
    },
  ];
}

const meta: Meta<NurseEntitlementDetail> = {
  component: NurseEntitlementDetail,
  title: 'Features/Nurse/PreparationPackages/EntitlementDetail',
};
export default meta;
type Story = StoryObj<NurseEntitlementDetail>;

export const Active: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new DetailApi(ACTIVE)) })],
};

export const Expired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new DetailApi(EXPIRED)) })],
};

export const MultipleRights: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new DetailApi(MULTIPLE_RIGHTS)) })],
};

export const Missing: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingApi()) })],
};
