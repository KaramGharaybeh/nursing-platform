import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { adminArchiveExam } from './generated/fn/nursing-platform-web-api/admin-archive-exam';
import { adminCreateExam } from './generated/fn/nursing-platform-web-api/admin-create-exam';
import { adminDeleteExam } from './generated/fn/nursing-platform-web-api/admin-delete-exam';
import { adminGetExam } from './generated/fn/nursing-platform-web-api/admin-get-exam';
import { adminListExams } from './generated/fn/nursing-platform-web-api/admin-list-exams';
import type { AdminListExams$Params } from './generated/fn/nursing-platform-web-api/admin-list-exams';
import { adminUpdateExam } from './generated/fn/nursing-platform-web-api/admin-update-exam';
import type { AdminExamDto } from './generated/models/admin-exam-dto';
import type { CreateAdminExamRequest } from './generated/models/create-admin-exam-request';
import type { PaginatedResultOfAdminExamDto } from './generated/models/paginated-result-of-admin-exam-dto';
import type { UpdateAdminExamRequest } from './generated/models/update-admin-exam-request';

@Injectable({ providedIn: 'root' })
export class AdminExamsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  list(params: Omit<AdminListExams$Params, 'pageSize'>): Observable<PaginatedResultOfAdminExamDto> {
    return adminListExams(this.http, this.config.rootUrl, { ...params, pageSize: 20 }).pipe(
      map((response) => response.body),
    );
  }

  get(id: string): Observable<AdminExamDto> {
    return adminGetExam(this.http, this.config.rootUrl, { id }).pipe(map((response) => response.body));
  }

  create(body: CreateAdminExamRequest): Observable<AdminExamDto> {
    return adminCreateExam(this.http, this.config.rootUrl, { body }).pipe(map((response) => response.body));
  }

  update(id: string, body: UpdateAdminExamRequest): Observable<AdminExamDto> {
    return adminUpdateExam(this.http, this.config.rootUrl, { id, body }).pipe(map((response) => response.body));
  }

  archive(id: string): Observable<AdminExamDto> {
    return adminArchiveExam(this.http, this.config.rootUrl, { id }).pipe(map((response) => response.body));
  }

  delete(id: string): Observable<void> {
    return adminDeleteExam(this.http, this.config.rootUrl, { id }).pipe(map(() => undefined));
  }
}
