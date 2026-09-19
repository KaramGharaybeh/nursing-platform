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

/**
 * T-FE-079 proving evidence: the package exam trio delegates to the generated
 * entitlement operations with exact path parameters and maps strict bodies.
 * The consumptive start is never called except through explicit user action
 * covered by component tests; the facade itself only forwards.
 */
describe('preparation-package-entitlements-api package exam (T-FE-079)', () => {
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

  it('gets package exam session state by entitlement id', async () => {
    const expected = {
      hasSession: true,
      sessionId: 'sess-1',
      examId: 'exam-1',
      status: 'InProgress',
      expiresAt: '2999-01-01T00:00:00Z',
    };

    const result = firstValueFrom(api.getPackageExamSessionState('ent-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/exam-session',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });

  it('starts a package exam session with the exact entitlement id', async () => {
    const result = firstValueFrom(api.startPackageExamSession('ent-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/exam-session',
    );

    expect(request.request.method).toBe('POST');
    request.flush({
      session: { id: 'sess-9', examId: 'exam-1' },
      entitlementId: 'ent-1',
      includedExamId: 'exam-1',
    });
    await expect(result).resolves.toEqual({ sessionId: 'sess-9', examId: 'exam-1' });
  });

  it('rejects a package start response without session identity', async () => {
    const result = firstValueFrom(api.startPackageExamSession('ent-1'));
    httpMock
      .expectOne(
        '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/exam-session',
      )
      .flush({ session: null, entitlementId: 'ent-1', includedExamId: 'exam-1' });

    await expect(result).rejects.toThrow(
      'Package exam start response did not include a session identity.',
    );
  });

  it('gets a package analytical report by session id', async () => {
    const result = firstValueFrom(api.getPackageAnalyticalReport('sess-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/exam-sessions/sess-1/report',
    );

    expect(request.request.method).toBe('GET');
    request.flush({
      id: 'rep-1',
      examSessionId: 'sess-1',
      score: 1,
      maxScore: 2,
      percentage: 50,
      passed: false,
      correctCount: 1,
      questionCount: 2,
      topicResults: [],
      guidanceItems: [],
    });
    const report = await result;
    expect(report.score).toBe(1);
    expect(report.questionCount).toBe(2);
  });
});
