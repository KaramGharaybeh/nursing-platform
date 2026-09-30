import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { adminArchiveExamCategory } from './generated/fn/nursing-platform-web-api/admin-archive-exam-category';
import { adminCreateExamCategory } from './generated/fn/nursing-platform-web-api/admin-create-exam-category';
import { adminDeleteExamCategory } from './generated/fn/nursing-platform-web-api/admin-delete-exam-category';
import { adminGetExamCategory } from './generated/fn/nursing-platform-web-api/admin-get-exam-category';
import { adminListExamCategories } from './generated/fn/nursing-platform-web-api/admin-list-exam-categories';
import type { AdminListExamCategories$Params } from './generated/fn/nursing-platform-web-api/admin-list-exam-categories';
import { adminRestoreExamCategory } from './generated/fn/nursing-platform-web-api/admin-restore-exam-category';
import { adminUpdateExamCategory } from './generated/fn/nursing-platform-web-api/admin-update-exam-category';
import type { AdminExamCategoryDto } from './generated/models/admin-exam-category-dto';
import type { CreateAdminExamCategoryRequest } from './generated/models/create-admin-exam-category-request';
import type { PaginatedResultOfAdminExamCategoryDto } from './generated/models/paginated-result-of-admin-exam-category-dto';
import type { UpdateAdminExamCategoryRequest } from './generated/models/update-admin-exam-category-request';

@Injectable({ providedIn: 'root' })
export class AdminExamCategoriesApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  list(params: AdminListExamCategories$Params): Observable<PaginatedResultOfAdminExamCategoryDto> {
    return adminListExamCategories(this.http, this.config.rootUrl, params).pipe(map((response) => response.body));
  }

  get(id: string): Observable<AdminExamCategoryDto> {
    return adminGetExamCategory(this.http, this.config.rootUrl, { id }).pipe(map((response) => response.body));
  }

  create(body: CreateAdminExamCategoryRequest): Observable<AdminExamCategoryDto> {
    return adminCreateExamCategory(this.http, this.config.rootUrl, { body }).pipe(map((response) => response.body));
  }

  update(id: string, body: UpdateAdminExamCategoryRequest): Observable<AdminExamCategoryDto> {
    return adminUpdateExamCategory(this.http, this.config.rootUrl, { id, body }).pipe(map((response) => response.body));
  }

  archive(id: string): Observable<AdminExamCategoryDto> {
    return adminArchiveExamCategory(this.http, this.config.rootUrl, { id }).pipe(map((response) => response.body));
  }

  restore(id: string): Observable<AdminExamCategoryDto> {
    return adminRestoreExamCategory(this.http, this.config.rootUrl, { id }).pipe(map((response) => response.body));
  }

  delete(id: string): Observable<void> {
    return adminDeleteExamCategory(this.http, this.config.rootUrl, { id }).pipe(map(() => undefined));
  }
}
