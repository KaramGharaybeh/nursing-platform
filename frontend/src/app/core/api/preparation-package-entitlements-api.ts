import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getMyPackageAnalyticalReport } from './generated/fn/preparation-package-entitlements/get-my-package-analytical-report';
import { getMyPackageEntitlement } from './generated/fn/preparation-package-entitlements/get-my-package-entitlement';
import { getMyPackageExamSessionState } from './generated/fn/preparation-package-entitlements/get-my-package-exam-session-state';
import { listMyPackageEntitlements } from './generated/fn/preparation-package-entitlements/list-my-package-entitlements';
import { startPackageExamSession } from './generated/fn/preparation-package-entitlements/start-package-exam-session';
import type { PackageAnalyticalReportDto } from './generated/models/package-analytical-report-dto';
import type { PackageEntitlementDetailDto } from './generated/models/package-entitlement-detail-dto';
import type { PackageExamSessionStateDto } from './generated/models/package-exam-session-state-dto';
import type { PaginatedResultOfPackageEntitlementListItemDto } from './generated/models/paginated-result-of-package-entitlement-list-item-dto';

export interface PackageExamStart {
  readonly sessionId: string;
  readonly examId: string;
}

export interface PackageEntitlementQuery {
  readonly page: number;
  readonly pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class PreparationPackageEntitlementsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  listMyEntitlements(query: PackageEntitlementQuery): Observable<PaginatedResultOfPackageEntitlementListItemDto> {
    return listMyPackageEntitlements(this.http, this.config.rootUrl, {
      page: query.page,
      pageSize: query.pageSize,
    }).pipe(map((response) => response.body));
  }

  getMyEntitlement(entitlementId: string): Observable<PackageEntitlementDetailDto> {
    return getMyPackageEntitlement(this.http, this.config.rootUrl, { id: entitlementId }).pipe(
      map((response) => response.body),
    );
  }

  getPackageExamSessionState(entitlementId: string): Observable<PackageExamSessionStateDto> {
    return getMyPackageExamSessionState(this.http, this.config.rootUrl, { entitlementId }).pipe(
      map((response) => response.body),
    );
  }

  startPackageExamSession(entitlementId: string): Observable<PackageExamStart> {
    return startPackageExamSession(this.http, this.config.rootUrl, { entitlementId }).pipe(
      map((response) => {
        const sessionId = response.body.session?.id ?? '';
        const examId = response.body.includedExamId ?? '';
        if (sessionId === '' || examId === '') {
          throw new Error('Package exam start response did not include a session identity.');
        }
        return { sessionId, examId };
      }),
    );
  }

  getPackageAnalyticalReport(sessionId: string): Observable<PackageAnalyticalReportDto> {
    return getMyPackageAnalyticalReport(this.http, this.config.rootUrl, { sessionId }).pipe(
      map((response) => response.body),
    );
  }
}
