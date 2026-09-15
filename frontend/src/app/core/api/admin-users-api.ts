import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getUser } from './generated/fn/nursing-platform-web-api/get-user';
import { listUsers } from './generated/fn/nursing-platform-web-api/list-users';
import type { ListUsers$Params } from './generated/fn/nursing-platform-web-api/list-users';
import { updateAdminUserRole } from './generated/fn/nursing-platform-web-api/update-admin-user-role';
import type { PaginatedResultOfUserListItemDto } from './generated/models/paginated-result-of-user-list-item-dto';
import type { UpdateUserRolesRequest } from './generated/models/update-user-roles-request';
import type { UpdateUserRolesResponse } from './generated/models/update-user-roles-response';
import type { UserDetailDto } from './generated/models/user-detail-dto';

@Injectable({ providedIn: 'root' })
export class AdminUsersApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  list(params?: ListUsers$Params): Observable<PaginatedResultOfUserListItemDto> {
    return listUsers(this.http, this.config.rootUrl, params).pipe(
      map((response) => response.body),
    );
  }

  get(userId: string): Observable<UserDetailDto> {
    return getUser(this.http, this.config.rootUrl, { id: userId }).pipe(
      map((response) => response.body),
    );
  }

  updateRole(userId: string, request: UpdateUserRolesRequest): Observable<UpdateUserRolesResponse> {
    return updateAdminUserRole(this.http, this.config.rootUrl, { userId, body: request }).pipe(
      map((response) => response.body),
    );
  }
}
