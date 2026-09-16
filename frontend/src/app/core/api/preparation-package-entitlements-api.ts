import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getMyPackageEntitlement } from './generated/fn/preparation-package-entitlements/get-my-package-entitlement';
import { listMyPackageEntitlements } from './generated/fn/preparation-package-entitlements/list-my-package-entitlements';
import type { PackageEntitlementDetailDto } from './generated/models/package-entitlement-detail-dto';
import type { PaginatedResultOfPackageEntitlementListItemDto } from './generated/models/paginated-result-of-package-entitlement-list-item-dto';

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
}
