import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { PreparationPackageEntitlementsApi } from './preparation-package-entitlements-api';
import type { PackageEntitlementDetailDto } from './generated/models/package-entitlement-detail-dto';
import type { PaginatedResultOfPackageEntitlementListItemDto } from './generated/models/paginated-result-of-package-entitlement-list-item-dto';

/**
 * T-FE-076 proving evidence: the thin nurse entitlement facade delegates to the
 * generated PP entitlement operations with exact query/path parameters and maps
 * the strict response body. No filter/search/sort parameters exist on the list
 * contract, so none are sent.
 */
describe('preparation-package-entitlements-api (T-FE-076)', () => {
  let api: PreparationPackageEntitlementsApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(PreparationPackageEntitlementsApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists entitlements with exact page and pageSize and no other query parameters', async () => {
    const expected: PaginatedResultOfPackageEntitlementListItemDto = {
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
    };

    const result = firstValueFrom(api.listMyEntitlements({ page: 1, pageSize: 20 }));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements?page=1&pageSize=20',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });

  it('gets one entitlement by route id through the generated detail operation', async () => {
    const expected: PackageEntitlementDetailDto = {
      id: 'ent-1',
      packageOfferId: 'offer-1',
      packageOfferTitle: 'Offer',
      packageDefinitionId: 'def-1',
      packageDefinitionTitle: 'Definition',
      packageVersionId: 'ver-1',
      includedExamId: 'exam-1',
      includedExamTitle: 'Exam',
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: '2026-11-30T00:00:00Z',
      status: 'Active',
      benefitRights: [],
      purchasedSnapshot: {},
    };

    const result = firstValueFrom(api.getMyEntitlement('ent-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });
});
