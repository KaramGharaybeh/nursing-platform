import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { PreparationPackageOffersApi } from './preparation-package-offers-api';
import type { PreparationPackageOfferDetailDto } from './generated/models/preparation-package-offer-detail-dto';
import type { PaginatedResultOfPreparationPackageOfferListItemDto } from './generated/models/paginated-result-of-preparation-package-offer-list-item-dto';

/**
 * T-FE-075 proving evidence: the thin public offers facade delegates to the
 * generated PP catalog operations with exact query/path parameters and maps
 * the strict response body. The list UI ships pagination only, so the facade
 * sends page/pageSize and no filter/search/sort parameters.
 */
describe('preparation-package-offers-api (T-FE-075)', () => {
  let api: PreparationPackageOffersApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(PreparationPackageOffersApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists offers with exact page and pageSize and no other query parameters', async () => {
    const expected: PaginatedResultOfPreparationPackageOfferListItemDto = {
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
    };

    const result = firstValueFrom(api.listOffers({ page: 1, pageSize: 20 }));
    const request = httpMock.expectOne('/api/v1/preparation-packages/offers?page=1&pageSize=20');

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });

  it('gets one offer by slug through the generated detail operation', async () => {
    const expected: PreparationPackageOfferDetailDto = {
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
      components: [],
    };

    const result = firstValueFrom(api.getOffer('nclex-preparation-package'));
    const request = httpMock.expectOne(
      '/api/v1/preparation-packages/offers/nclex-preparation-package',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });
});
