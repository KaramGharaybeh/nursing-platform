import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { approveReceivedContactRequest } from './generated/fn/nursing-platform-web-api/approve-received-contact-request';
import { listReceivedContactRequests } from './generated/fn/nursing-platform-web-api/list-received-contact-requests';
import { rejectReceivedContactRequest } from './generated/fn/nursing-platform-web-api/reject-received-contact-request';
import type { ContactRequestStatus } from './generated/models/contact-request-status';
import type { PaginatedResultOfReceivedContactRequestDto } from './generated/models/paginated-result-of-received-contact-request-dto';
import type { ReceivedContactRequestDto } from './generated/models/received-contact-request-dto';

export interface ReceivedContactRequestQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly status?: ContactRequestStatus;
}

@Injectable({ providedIn: 'root' })
export class ContactRequestsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  listReceived(query: ReceivedContactRequestQuery): Observable<PaginatedResultOfReceivedContactRequestDto> {
    return listReceivedContactRequests(this.http, this.config.rootUrl, {
      page: query.page,
      pageSize: query.pageSize,
      status: query.status,
    }).pipe(map((response) => response.body));
  }

  approve(id: string): Observable<ReceivedContactRequestDto> {
    return approveReceivedContactRequest(this.http, this.config.rootUrl, { id }).pipe(
      map((response) => response.body),
    );
  }

  reject(id: string): Observable<ReceivedContactRequestDto> {
    return rejectReceivedContactRequest(this.http, this.config.rootUrl, { id }).pipe(
      map((response) => response.body),
    );
  }
}
