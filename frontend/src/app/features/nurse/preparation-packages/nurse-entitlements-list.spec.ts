import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import type { PackageEntitlementListItemDto } from '../../../core/api/generated/models/package-entitlement-list-item-dto';
import type { PaginatedResultOfPackageEntitlementListItemDto } from '../../../core/api/generated/models/paginated-result-of-package-entitlement-list-item-dto';
import { NurseEntitlementsList } from './nurse-entitlements-list';

const ACTIVE: PackageEntitlementListItemDto = {
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

const EXPIRED: PackageEntitlementListItemDto = {
  id: 'ent-expired-2',
  packageOfferId: 'offer-2',
  packageOfferTitle: 'CGFNS Qualifying Preparation with a very long offer title that must wrap safely',
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

function pageOf(
  items: PackageEntitlementListItemDto[],
  totalCount: number,
  page = 1,
): PaginatedResultOfPackageEntitlementListItemDto {
  return {
    items,
    page,
    pageSize: 20,
    totalCount,
    totalPages: Math.max(1, Math.ceil(totalCount / 20)),
  };
}

class EntitlementsApiStub {
  pages: PaginatedResultOfPackageEntitlementListItemDto = pageOf([ACTIVE, EXPIRED], 2);
  pagesByPage: Record<number, PaginatedResultOfPackageEntitlementListItemDto> = {};
  listError: unknown = undefined;
  listed: { page: number; pageSize: number }[] = [];

  listMyEntitlements(query: { page: number; pageSize: number }) {
    this.listed.push({ ...query });
    if (this.listError !== undefined) {
      return throwError(() => this.listError);
    }
    const page = this.pagesByPage[query.page] ?? this.pages;
    return of({ ...page, page: query.page });
  }

  getMyEntitlement() {
    return throwError(() => ({ status: 404 }));
  }
}

async function setup(stub?: EntitlementsApiStub): Promise<{ fixture: ComponentFixture<NurseEntitlementsList>; api: EntitlementsApiStub }> {
  const api = stub ?? new EntitlementsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseEntitlementsList],
    providers: [provideRouter([]), { provide: PreparationPackageEntitlementsApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseEntitlementsList);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api };
}

function text(fixture: ComponentFixture<NurseEntitlementsList>): string {
  return fixture.nativeElement.textContent as string;
}

function allByTestId(fixture: ComponentFixture<NurseEntitlementsList>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseEntitlementsList>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseEntitlementsList', () => {
  it('loads page 1 with pageSize 20 and sends no filter/search/sort parameters', async () => {
    const { fixture, api } = await setup();

    expect(api.listed.length).toBe(1);
    expect(api.listed[0]).toEqual({ page: 1, pageSize: 20 });
    expect(allByTestId(fixture, 'entitlement-card').length).toBe(2);
  });

  it('renders package identity hierarchy with backend status and access dates', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('NCLEX Preparation Package');
    expect(content).toContain('NCLEX Prep Definition');
    expect(content).toContain('NCLEX-RN Readiness Exam');
    expect(content).toContain('Active');
    expect(content).toContain('Expired');
  });

  it('exposes no raw ids and no purchase/practice/report actions', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toContain('ent-active-1');
    expect(content).not.toContain('offer-1');
    expect(content).not.toContain('def-1');
    expect(content).not.toContain('exam-1');
    expect(content).not.toContain('Buy');
    expect(content).not.toContain('Checkout');
    expect(content).not.toContain('Upgrade');
    expect(content).not.toContain('Renew');
    expect(content).not.toContain('Start practice');
    expect(content).not.toContain('Continue practice');
    expect(content).not.toContain('View report');
    expect(content).not.toContain('Start exam');
  });

  it('renders per-right availability facts from backend truth', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('Study material');
    expect(content).toContain('Available');
    expect(content).toContain('Not available');
  });

  it('links each card to its canonical detail route', async () => {
    const { fixture } = await setup();
    const links = allByTestId(fixture, 'entitlement-details-link') as HTMLAnchorElement[];

    expect(links.length).toBe(2);
    expect(links[0]?.getAttribute('href')).toBe('/nurse/preparation-packages/ent-active-1');
    expect(links[1]?.getAttribute('href')).toBe('/nurse/preparation-packages/ent-expired-2');
  });

  it('shows the calm empty state with no call to action when the nurse owns nothing', async () => {
    const stub = new EntitlementsApiStub();
    stub.pages = pageOf([], 0);
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(content).toContain('No preparation packages yet.');
    expect(allByTestId(fixture, 'entitlement-card').length).toBe(0);
    expect(content).not.toContain('Browse');
    expect(content).not.toContain('Buy');
  });

  it('passes the requested page through to pagination and preserves it on retry', async () => {
    const stub = new EntitlementsApiStub();
    stub.listError = { status: 500, error: { title: 'Server error', detail: 'Try again.' } };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as {
      loadPage(page: number): Promise<void>;
      retry(): Promise<void>;
    };

    stub.listError = undefined;
    await component.loadPage(3);
    await settle(fixture);
    expect(api.listed[api.listed.length - 1]).toEqual({ page: 3, pageSize: 20 });

    stub.listError = { status: 500, error: { title: 'Server error', detail: 'Try again.' } };
    stub.pages = pageOf([], 0);
    await component.loadPage(3);
    await settle(fixture);
    stub.listError = undefined;
    await component.retry();
    await settle(fixture);
    expect(api.listed[api.listed.length - 1]).toEqual({ page: 3, pageSize: 20 });
  });

  it('loads the server totalPages when a requested page is out of range', async () => {
    const stub = new EntitlementsApiStub();
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as {
      loadPage(page: number): Promise<void>;
    };

    stub.pages = pageOf([], 40, 2);
    stub.pagesByPage = { 2: pageOf([ACTIVE], 40, 2) };
    await component.loadPage(3);
    await settle(fixture);

    expect(api.listed[api.listed.length - 1]).toEqual({ page: 2, pageSize: 20 });
    const content = text(fixture);
    expect(content).not.toContain('No preparation packages yet.');
    expect(content).toContain('NCLEX Preparation Package');
  });
});

describe('Nurse entitlements list route', () => {
  it('mounts /nurse/preparation-packages with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/preparation-packages');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_ENTITLEMENTS' });
  });
});
