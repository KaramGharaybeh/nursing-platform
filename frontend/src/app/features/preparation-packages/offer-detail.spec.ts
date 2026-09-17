import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import type { PreparationPackageOfferDetailDto } from '../../core/api/generated/models/preparation-package-offer-detail-dto';
import { OfferDetail } from './offer-detail';

const DETAIL: PreparationPackageOfferDetailDto = {
  id: 'offer-1',
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
  components: [
    { name: 'Study materials', count: 12, summary: 'Concise review notes.' },
    { name: 'Practice questions', count: 240, summary: null },
  ],
};

class OffersApiStub {
  detail: PreparationPackageOfferDetailDto = DETAIL;
  detailError: unknown = undefined;
  requested: string[] = [];

  listOffers() {
    return throwError(() => ({ status: 500 }));
  }

  getOffer(offerSlug: string) {
    this.requested.push(offerSlug);
    if (this.detailError !== undefined) {
      return throwError(() => this.detailError);
    }
    return of(this.detail);
  }
}

async function setup(stub?: OffersApiStub): Promise<{ fixture: ComponentFixture<OfferDetail>; api: OffersApiStub }> {
  const api = stub ?? new OffersApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [OfferDetail],
    providers: [
      provideRouter([]),
      { provide: PreparationPackageOffersApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ offerSlug: 'nclex-preparation-package' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(OfferDetail);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api };
}

function text(fixture: ComponentFixture<OfferDetail>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<OfferDetail>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

async function settle(fixture: ComponentFixture<OfferDetail>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('OfferDetail', () => {
  it('loads the exact offer named by the route slug parameter', async () => {
    const { fixture, api } = await setup();

    expect(api.requested).toEqual(['nclex-preparation-package']);
    const content = text(fixture);
    expect(content).toContain('NCLEX Preparation Package');
    expect(content).toContain('Pass the NCLEX-RN with confidence.');
    expect(content).toContain('Jordan');
    expect(content).toContain('Nursing Licensure');
    expect(content).toContain('NCLEX-RN Readiness Exam');
  });

  it('renders component summaries with counts from backend truth', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('Study materials');
    expect(content).toContain('12');
    expect(content).toContain('Concise review notes.');
    expect(content).toContain('Practice questions');
    expect(content).toContain('240');
  });

  it('exposes no raw ids, no price, and no purchase or commerce actions', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toContain('offer-1');
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

  it('shows a contextual not-found notice with a safe back link for unknown slugs', async () => {
    const stub = new OffersApiStub();
    stub.detailError = { status: 404, error: { title: 'Not found', detail: 'Missing.' } };
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(byTestId(fixture, 'offer-missing-notice')).not.toBeNull();
    expect(content).toContain('no longer available');
    const back = byTestId(fixture, 'offer-back-link') as HTMLAnchorElement | null;
    expect(back).not.toBeNull();
    expect(back?.getAttribute('href')).toBe('/preparation-packages');
    expect(content).not.toContain('NCLEX Preparation Package');
  });

  it('surfaces backend errors with retry and reloads the same slug on retry', async () => {
    const stub = new OffersApiStub();
    stub.detailError = { status: 500, error: { title: 'Server error', detail: 'Try again.' } };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    stub.detailError = undefined;
    await component.retry();
    await settle(fixture);

    expect(api.requested).toEqual(['nclex-preparation-package', 'nclex-preparation-package']);
    expect(text(fixture)).toContain('NCLEX Preparation Package');
  });
});

describe('preparation packages offer detail route', () => {
  it('mounts /preparation-packages/:offerSlug as a public route with no guards and routeId', () => {
    const route = routes.find((entry) => entry.path === 'preparation-packages/:offerSlug');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate).toBeUndefined();
    expect(route?.canMatch).toBeUndefined();
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_OFFER_DETAIL' });
  });
});
