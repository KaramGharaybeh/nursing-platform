import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getPackagePracticeItems } from './generated/fn/preparation-package-entitlements/get-package-practice-items';
import { getPackagePracticeProgress } from './generated/fn/preparation-package-entitlements/get-package-practice-progress';
import { submitPackagePracticeAnswer } from './generated/fn/preparation-package-entitlements/submit-package-practice-answer';
import type { PackagePracticeAnswerSubmissionDto } from './generated/models/package-practice-answer-submission-dto';
import type { PackagePracticeContentListDto } from './generated/models/package-practice-content-list-dto';
import type { PackagePracticeProgressSummaryDto } from './generated/models/package-practice-progress-summary-dto';

@Injectable({ providedIn: 'root' })
export class PreparationPackagePracticeApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  getItems(entitlementId: string): Observable<PackagePracticeContentListDto> {
    return getPackagePracticeItems(this.http, this.config.rootUrl, { entitlementId }).pipe(
      map((response) => response.body),
    );
  }

  getProgress(entitlementId: string): Observable<PackagePracticeProgressSummaryDto> {
    return getPackagePracticeProgress(this.http, this.config.rootUrl, { entitlementId }).pipe(
      map((response) => response.body),
    );
  }

  submitAnswer(
    entitlementId: string,
    practiceItemId: string,
    selectedPracticeAnswerOptionId: string,
  ): Observable<PackagePracticeAnswerSubmissionDto> {
    return submitPackagePracticeAnswer(this.http, this.config.rootUrl, {
      entitlementId,
      practiceItemId,
      body: { selectedPracticeAnswerOptionId },
    }).pipe(map((response) => response.body));
  }
}
