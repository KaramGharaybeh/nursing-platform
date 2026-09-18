import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { PreparationPackagePracticeApi } from './preparation-package-practice-api';
import type { PackagePracticeAnswerSubmissionDto } from './generated/models/package-practice-answer-submission-dto';
import type { PackagePracticeContentListDto } from './generated/models/package-practice-content-list-dto';
import type { PackagePracticeProgressSummaryDto } from './generated/models/package-practice-progress-summary-dto';

/**
 * T-FE-078 proving evidence: the thin nurse practice facade delegates to the
 * generated preparation-package entitlement operations with exact path
 * parameters and maps the strict response bodies. No filter/search/sort or
 * paging parameters exist on these learner practice contracts.
 */
describe('preparation-package-practice-api (T-FE-078)', () => {
  let api: PreparationPackagePracticeApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(PreparationPackagePracticeApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('gets learner practice items by entitlement id through the generated operation', async () => {
    const expected: PackagePracticeContentListDto = {
      packagePurchaseEntitlementId: 'ent-1',
      practiceCollectionVersionId: 'ver-1',
      totalItems: 1,
      items: [],
    };

    const result = firstValueFrom(api.getItems('ent-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/practice-progress/items',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });

  it('gets practice progress by entitlement id through the generated operation', async () => {
    const expected: PackagePracticeProgressSummaryDto = {
      packagePurchaseEntitlementId: 'ent-1',
      practiceCollectionVersionId: 'ver-1',
      totalItems: 1,
      answeredCount: 0,
      unansweredCount: 1,
      correctCount: 0,
      incorrectCount: 0,
      itemStates: [],
    };

    const result = firstValueFrom(api.getProgress('ent-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/practice-progress',
    );

    expect(request.request.method).toBe('GET');
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });

  it('submits an answer with exact route ids and option body', async () => {
    const expected: PackagePracticeAnswerSubmissionDto = {
      practiceItemId: 'item-1',
      state: 1,
      selectedPracticeAnswerOptionId: 'opt-1',
      lastAnsweredAt: '2026-09-18T00:00:00Z',
      immediateFeedback: 'Feedback',
    };

    const result = firstValueFrom(api.submitAnswer('ent-1', 'item-1', 'opt-1'));
    const request = httpMock.expectOne(
      '/api/v1/me/nurse-profile/preparation-packages/entitlements/ent-1/practice-progress/items/item-1/answer',
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ selectedPracticeAnswerOptionId: 'opt-1' });
    request.flush(expected);
    await expect(result).resolves.toEqual(expected);
  });
});
