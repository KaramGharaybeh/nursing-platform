import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getPreparationPackageOffer } from './generated/fn/preparation-package-catalog/get-preparation-package-offer';
import { listPreparationPackageOffers } from './generated/fn/preparation-package-catalog/list-preparation-package-offers';
import type { PreparationPackageOfferDetailDto } from './generated/models/preparation-package-offer-detail-dto';
import type { PaginatedResultOfPreparationPackageOfferListItemDto } from './generated/models/paginated-result-of-preparation-package-offer-list-item-dto';

export interface PreparationPackageOfferQuery {
  readonly page: number;
  readonly pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class PreparationPackageOffersApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  listOffers(query: PreparationPackageOfferQuery): Observable<PaginatedResultOfPreparationPackageOfferListItemDto> {
    return listPreparationPackageOffers(this.http, this.config.rootUrl, {
      page: query.page,
      pageSize: query.pageSize,
    }).pipe(map((response) => response.body));
  }

  getOffer(offerSlug: string): Observable<PreparationPackageOfferDetailDto> {
    return getPreparationPackageOffer(this.http, this.config.rootUrl, { slug: offerSlug }).pipe(
      map((response) => response.body),
    );
  }
}
