import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { PreparationPackageOffersApi } from '../../core/api/preparation-package-offers-api';
import { OffersList } from './offers-list';

const NCLEX = {
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

const CGFNS = {
  id: 'offer-cgfns-2',
  title: 'CGFNS Qualifying Preparation',
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

const LONG_TITLE = {
  ...NCLEX,
  id: 'offer-long-3',
  slug: 'exceptionally-long-offer-slug-for-wrapping',
  title:
    'An exceptionally long preparation package offer title that keeps going so wrapping behavior stays safe on narrow screens',
  summary:
    'An exceptionally long offer summary that keeps going so wrapping behavior stays safe on narrow screens.',
};

function pageOf(items: typeof NCLEX[], totalCount: number, totalPages: number) {
  return of({
    items,
    page: 1,
    pageSize: 20,
    totalCount,
    totalPages,
  });
}

class PopulatedApi {
  listOffers() {
    return pageOf([NCLEX, CGFNS], 2, 1);
  }
}

class ManyPagesApi extends PopulatedApi {
  override listOffers() {
    return pageOf([NCLEX, CGFNS], 45, 3);
  }
}

class EmptyApi extends PopulatedApi {
  override listOffers() {
    return pageOf([], 0, 1);
  }
}

class LongTitlesApi extends PopulatedApi {
  override listOffers() {
    return pageOf([LONG_TITLE], 1, 1);
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: PreparationPackageOffersApi, useValue: api }];
}

const meta: Meta<OffersList> = {
  component: OffersList,
  title: 'Features/PreparationPackages/OffersList',
};
export default meta;
type Story = StoryObj<OffersList>;

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
