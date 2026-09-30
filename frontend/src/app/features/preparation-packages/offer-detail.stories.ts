import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import { OfferDetail } from './offer-detail';

const DETAIL = {
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

const NO_SUMMARY = {
  ...DETAIL,
  id: 'offer-2',
  slug: 'no-summary-offer',
  summary: null,
  components: [],
};

class PopulatedApi {
  getOffer() {
    return of(DETAIL);
  }
}

class NoSummaryApi extends PopulatedApi {
  override getOffer() {
    return of(NO_SUMMARY);
  }
}

class MissingApi extends PopulatedApi {
  override getOffer() {
    return throwError(() => ({ status: 404 }));
  }
}

function providers(api: PopulatedApi, offerSlug: string) {
  return [
    provideRouter([]),
    { provide: PreparationPackageOffersApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ offerSlug }) } },
    },
  ];
}

const meta: Meta<OfferDetail> = {
  component: OfferDetail,
  title: 'Features/PreparationPackages/OfferDetail',
};
export default meta;
type Story = StoryObj<OfferDetail>;

export const Populated: Story = {
  decorators: [
    (story) => ({ ...story(), providers: providers(new PopulatedApi(), 'nclex-preparation-package') }),
  ],
};

export const NoSummary: Story = {
  decorators: [
    (story) => ({ ...story(), providers: providers(new NoSummaryApi(), 'no-summary-offer') }),
  ],
};

export const Missing: Story = {
  decorators: [
    (story) => ({ ...story(), providers: providers(new MissingApi(), 'unknown-slug') }),
  ],
};
