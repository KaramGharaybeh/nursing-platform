import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import type { PreparationPackageOfferListItemDto } from '../../core/api/generated/models/preparation-package-offer-list-item-dto';
import type { PaginatedResultOfPreparationPackageOfferListItemDto } from '../../core/api/generated/models/paginated-result-of-preparation-package-offer-list-item-dto';
import { OffersList } from './offers-list';

const NCLEX: PreparationPackageOfferListItemDto = {
  id: 'offer-nclex-1',
  title: 'NCLEX Preparation Package',
  slug: 'nclex-preparation-package',
  summary: 'Pass the NCLEX-RN with confidence.',
  countryId: 'country-1',
  countryName: 'Jordan',
  examCategoryId: 'category-1',
  examCategoryName: 'Nursing Licensure',
  examId: 'exam-1',
  examTitle: 'NCLEX-RN Readiness Exam',
  materialCount: 12,
  practiceItemCount: 240,
  accessDurationDays: 90,
  priceAmountMinor: '4999',
  currency: 'USD',
};

const CGFNS: PreparationPackageOfferListItemDto = {
  id: 'offer-cgfns-2',
  title: 'CGFNS Qualifying Preparation with a very long offer title that must wrap safely',
  slug: 'cgfns-qualifying-preparation',
  summary: null,
  countryId: 'country-2',
  countryName: 'United States',
  examCategoryId: 'category-2',
  examCategoryName: 'Credential Evaluation',
  examId: 'exam-2',
  examTitle: 'CGFNS Qualifying Exam',
  materialCount: 8,
  practiceItemCount: 120,
  accessDurationDays: 60,
  priceAmountMinor: '2999',
  currency: 'USD',
};

function pageOf(
  items: PreparationPackageOfferListItemDto[],
  totalCount: number,
  page = 1,
): PaginatedResultOfPreparationPackageOfferListItemDto {
  return {
    items,
    page,
    pageSize: 20,
    totalCount,
    totalPages: Math.max(1, Math.ceil(totalCount / 20)),
  };
}

class OffersApiStub {
  pages: PaginatedResultOfPreparationPackageOfferListItemDto = pageOf([NCLEX, CGFNS], 2);
  pagesByPage: Record<number, PaginatedResultOfPreparationPackageOfferListItemDto> = {};
  listError: unknown = undefined;
  listed: { page: number; pageSize: number }[] = [];

  listOffers(query: { page: number; pageSize: number }) {
    this.listed.push({ ...query });
    if (this.listError !== undefined) {
      return throwError(() => this.listError);
    }
    const page = this.pagesByPage[query.page] ?? this.pages;
    return of({ ...page, page: query.page });
  }

  getOffer() {
    return throwError(() => ({ status: 404 }));
  }
}

async function setup(stub?: OffersApiStub): Promise<{ fixture: ComponentFixture<OffersList>; api: OffersApiStub }> {
  const api = stub ?? new OffersApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [OffersList],
    providers: [provideRouter([]), { provide: PreparationPackageOffersApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(OffersList);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api };
}

function text(fixture: ComponentFixture<OffersList>): string {
  return fixture.nativeElement.textContent as string;
}

function allByTestId(fixture: ComponentFixture<OffersList>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<OffersList>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('OffersList', () => {
  it('loads page 1 with pageSize 20 and sends no filter/search/sort parameters', async () => {
    const { fixture, api } = await setup();

    expect(api.listed.length).toBe(1);
    expect(api.listed[0]).toEqual({ page: 1, pageSize: 20 });
    expect(allByTestId(fixture, 'offer-card').length).toBe(2);
  });

  it('renders approved browse metadata from backend truth', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('NCLEX Preparation Package');
    expect(content).toContain('Pass the NCLEX-RN with confidence.');
    expect(content).toContain('Jordan');
    expect(content).toContain('Nursing Licensure');
    expect(content).toContain('NCLEX-RN Readiness Exam');
    expect(content).toContain('12');
    expect(content).toContain('240');
    expect(content).toContain('90');
  });

  it('exposes no raw ids, no price, and no purchase or commerce actions', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toContain('offer-nclex-1');
    expect(content).not.toContain('offer-cgfns-2');
    expect(content).not.toContain('country-1');
    expect(content).not.toContain('category-1');
    expect(content).not.toContain('exam-1');
    expect(content).not.toContain('4999');
    expect(content).not.toContain('USD');
    expect(content).not.toContain('Buy');
    expect(content).not.toContain('Purchase');
    expect(content).not.toContain('Checkout');
    expect(content).not.toContain('Add to cart');
    expect(content).not.toContain('Order');
  });

  it('links each card to its canonical detail route by slug', async () => {
    const { fixture } = await setup();
    const links = allByTestId(fixture, 'offer-details-link') as HTMLAnchorElement[];

    expect(links.length).toBe(2);
    expect(links[0]?.getAttribute('href')).toBe('/preparation-packages/nclex-preparation-package');
    expect(links[1]?.getAttribute('href')).toBe('/preparation-packages/cgfns-qualifying-preparation');
  });

  it('shows the calm empty state with no call to action when no offers exist', async () => {
    const stub = new OffersApiStub();
    stub.pages = pageOf([], 0);
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(content).toContain('No preparation packages available.');
    expect(allByTestId(fixture, 'offer-card').length).toBe(0);
    expect(content).not.toContain('Buy');
    expect(content).not.toContain('Purchase');
  });

  it('passes the requested page through to pagination and preserves it on retry', async () => {
    const stub = new OffersApiStub();
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
    const stub = new OffersApiStub();
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as {
      loadPage(page: number): Promise<void>;
    };

    stub.pages = pageOf([], 40, 2);
    stub.pagesByPage = { 2: pageOf([NCLEX], 40, 2) };
    await component.loadPage(3);
    await settle(fixture);

    expect(api.listed[api.listed.length - 1]).toEqual({ page: 2, pageSize: 20 });
    const content = text(fixture);
    expect(content).not.toContain('No preparation packages available.');
    expect(content).toContain('NCLEX Preparation Package');
  });
});

describe('preparation packages offers list route', () => {
  it('mounts /preparation-packages as a public route with no guards and routeId', () => {
    const route = routes.find((entry) => entry.path === 'preparation-packages');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate).toBeUndefined();
    expect(route?.canMatch).toBeUndefined();
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_OFFERS' });
  });
});
