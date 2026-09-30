import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { ExamsApi } from '../../../core/api/exams-api';
import type { PackageEntitlementDetailDto } from '../../../core/api/generated/models/package-entitlement-detail-dto';
import { NurseEntitlementDetail } from './nurse-entitlement-detail';

const DETAIL: PackageEntitlementDetailDto = {
  id: 'ent-1',
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
      rightType: 'ExamAttempt',
      status: 'Locked',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: null,
      isAvailable: false,
      isDormant: false,
    },
  ],
  purchasedSnapshot: {
    packageOfferSummary: 'Focused preparation for first-time test takers.',
  },
};

class EntitlementsApiStub {
  detail: PackageEntitlementDetailDto = DETAIL;
  detailError: unknown = undefined;
  requested: string[] = [];

  listMyEntitlements() {
    return throwError(() => ({ status: 500 }));
  }

  getMyEntitlement(entitlementId: string) {
    this.requested.push(entitlementId);
    if (this.detailError !== undefined) {
      return throwError(() => this.detailError);
    }
    return of(this.detail);
  }

  getPackageExamSessionState() {
    return of({ hasSession: false, sessionId: null, examId: null, status: null, expiresAt: null });
  }

  startPackageExamSession() {
    return throwError(() => ({ status: 409 }));
  }

  getPackageAnalyticalReport() {
    return throwError(() => ({ status: 404 }));
  }
}

class ExamsApiStub {
  getExamSession() {
    return throwError(() => ({ status: 500 }));
  }
}

async function setup(stub?: EntitlementsApiStub): Promise<{ fixture: ComponentFixture<NurseEntitlementDetail>; api: EntitlementsApiStub }> {
  const api = stub ?? new EntitlementsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseEntitlementDetail],
    providers: [
      provideRouter([]),
      { provide: PreparationPackageEntitlementsApi, useValue: api },
      { provide: ExamsApi, useValue: new ExamsApiStub() },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ entitlementId: 'ent-1' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseEntitlementDetail);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api };
}

function text(fixture: ComponentFixture<NurseEntitlementDetail>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<NurseEntitlementDetail>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

async function settle(fixture: ComponentFixture<NurseEntitlementDetail>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseEntitlementDetail', () => {
  it('loads the exact entitlement named by the route parameter', async () => {
    const { fixture, api } = await setup();

    expect(api.requested).toEqual(['ent-1']);
    const content = text(fixture);
    expect(content).toContain('NCLEX Preparation Package');
    expect(content).toContain('NCLEX Prep Definition');
    expect(content).toContain('NCLEX-RN Readiness Exam');
    expect(content).toContain('Active');
  });

  it('shows the offer summary only when the snapshot provides one', async () => {
    const { fixture } = await setup();

    expect(text(fixture)).toContain('Focused preparation for first-time test takers.');
  });

  it('omits the summary and exposes no internals when the snapshot is bare', async () => {
    const stub = new EntitlementsApiStub();
    stub.detail = {
      ...DETAIL,
      purchasedSnapshot: {
        priceAmountMinor: '14900',
        currency: 'usd',
        countryId: 'country-1',
      },
    };
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(content).not.toContain('Focused preparation');
    expect(content).not.toContain('ent-1');
    expect(content).not.toContain('offer-1');
    expect(content).not.toContain('14900');
    expect(content).not.toContain('usd');
    expect(content).not.toContain('country-1');
  });

  it('renders each benefit right from backend truth without derivation', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('Study material');
    expect(content).toContain('Exam attempt');
    expect(content).toContain('Locked');
    expect(content).toContain('Available');
    expect(content).toContain('Not available');
    expect(content).not.toContain('Ready');
    expect(content).not.toContain('Complete');
  });

  it('offers no material or purchase actions while the package exam section renders backend truth', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toContain('Open material');
    expect(content).not.toContain('Purchase');
    expect(content).not.toContain('Renew');
    expect(content).not.toContain('Upgrade');
    expect(fixture.nativeElement.querySelectorAll('button').length).toBe(0);
  });

  it('shows a contextual missing notice with a safe return link on 404', async () => {
    const stub = new EntitlementsApiStub();
    stub.detailError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(content).toContain('no longer available');
    const back = byTestId(fixture, 'entitlement-back-link') as HTMLAnchorElement | null;
    expect(back).not.toBeNull();
    expect(back?.getAttribute('href')).toBe('/nurse/preparation-packages');
    expect(content).not.toContain('not yours');
    expect(content).not.toContain('permission');
  });

  it('retries the same entitlement after a server error', async () => {
    const stub = new EntitlementsApiStub();
    stub.detailError = { status: 500, error: { title: 'Server error', detail: 'Try again.' } };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    expect(text(fixture)).toContain('Try again');

    stub.detailError = undefined;
    await component.retry();
    await settle(fixture);

    expect(api.requested).toEqual(['ent-1', 'ent-1']);
    expect(text(fixture)).toContain('NCLEX Preparation Package');
  });

  it('shows the Practice link only when PracticeAccess is Available', async () => {
    const stub = new EntitlementsApiStub();
    stub.detail = {
      ...DETAIL,
      benefitRights: [
        ...DETAIL.benefitRights,
        {
          rightType: 'PracticeAccess',
          status: 'Available',
          accessStartsAt: '2026-09-01T00:00:00Z',
          accessEndsAt: '2026-11-30T00:00:00Z',
          isAvailable: true,
          isDormant: false,
        },
      ],
    };
    const { fixture } = await setup(stub);
    const link = byTestId(fixture, 'entitlement-practice-link') as HTMLAnchorElement | null;

    expect(link).not.toBeNull();
    expect(link?.textContent).toContain('Practice');
    expect(link?.getAttribute('href')).toBe('/nurse/preparation-packages/ent-1/practice');
  });

  it('hides the Practice link when PracticeAccess is missing, dormant, or unavailable', async () => {
    const variants: PackageEntitlementDetailDto['benefitRights'][] = [
      DETAIL.benefitRights,
      [
        {
          rightType: 'PracticeAccess',
          status: 'Dormant',
          accessStartsAt: '2026-09-01T00:00:00Z',
          accessEndsAt: null,
          isAvailable: false,
          isDormant: true,
        },
      ],
      [
        {
          rightType: 'PracticeAccess',
          status: 'Expired',
          accessStartsAt: '2026-09-01T00:00:00Z',
          accessEndsAt: '2026-09-02T00:00:00Z',
          isAvailable: false,
          isDormant: false,
        },
      ],
    ];

    for (const benefitRights of variants) {
      const stub = new EntitlementsApiStub();
      stub.detail = { ...DETAIL, benefitRights };
      const { fixture } = await setup(stub);

      expect(byTestId(fixture, 'entitlement-practice-link')).toBeNull();
    }
  });
});

describe('Nurse entitlement detail route', () => {
  it('mounts /nurse/preparation-packages/:entitlementId with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/preparation-packages/:entitlementId');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_ENTITLEMENT_DETAIL' });
  });
});
