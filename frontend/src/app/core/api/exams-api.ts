import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getExam } from './generated/fn/nursing-platform-web-api/get-exam';
import { listCountries } from './generated/fn/nursing-platform-web-api/list-countries';
import { listExams } from './generated/fn/nursing-platform-web-api/list-exams';

export interface ExamCatalogItem {
  readonly id: string;
  readonly title: string;
  readonly description: string | null;
  readonly countryId: string;
  readonly countryName: string;
  readonly categoryId: string | null;
  readonly categoryName: string | null;
  readonly durationMinutes: number;
  readonly questionCount: number;
  readonly passingScorePercentage: number;
  readonly isFree: boolean;
  readonly canStart: boolean;
}

export interface ExamCatalogPage {
  readonly items: ExamCatalogItem[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface ExamDetail extends ExamCatalogItem {
  readonly instructions: string | null;
}

export interface CountryOption {
  readonly id: string;
  readonly name: string;
}

export interface ExamCatalogQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly countryId?: string;
  readonly categoryId?: string;
}

@Injectable({ providedIn: 'root' })
export class ExamsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  listExams(query: ExamCatalogQuery): Observable<ExamCatalogPage> {
    return listExams(this.http, this.config.rootUrl, {
      page: query.page,
      pageSize: query.pageSize,
      countryId: query.countryId,
      categoryId: query.categoryId,
    }).pipe(map((response) => adaptCatalogPage(response.body)));
  }

  getExam(examId: string): Observable<ExamDetail> {
    return getExam(this.http, this.config.rootUrl, { id: examId }).pipe(
      map((response) => adaptExamDetail(response.body)),
    );
  }

  listCountries(): Observable<CountryOption[]> {
    return listCountries(this.http, this.config.rootUrl).pipe(
      map((response) =>
        response.body.map((country) => ({ id: country.id, name: country.name })),
      ),
    );
  }
}

function adaptCatalogPage(body: unknown): ExamCatalogPage {
  const page = asRecord(body);
  const items = Array.isArray(page['items']) ? page['items'].map(adaptCatalogItem) : [];
  return {
    items,
    page: asNumber(page['page'], 0),
    pageSize: asNumber(page['pageSize'], 0),
    totalCount: asNumber(page['totalCount'], 0),
    totalPages: asNumber(page['totalPages'], 0),
  };
}

function adaptCatalogItem(item: unknown): ExamCatalogItem {
  const record = asRecord(item);
  return {
    id: asString(record['id']),
    title: asString(record['title']),
    description: asNullableString(record['description']),
    countryId: asString(record['countryId']),
    countryName: asString(record['countryName']),
    categoryId: asNullableString(record['categoryId']),
    categoryName: asNullableString(record['categoryName']),
    durationMinutes: asNumber(record['durationMinutes'], 0),
    questionCount: asNumber(record['questionCount'], 0),
    passingScorePercentage: asNumber(record['passingScorePercentage'], 0),
    isFree: record['isFree'] === true,
    canStart: record['canStart'] === true,
  };
}

function adaptExamDetail(body: unknown): ExamDetail {
  const record = asRecord(body);
  return {
    ...adaptCatalogItem(body),
    instructions: asNullableString(record['instructions']),
  };
}

function asRecord(value: unknown): Record<string, unknown> {
  return typeof value === 'object' && value !== null
    ? (value as Record<string, unknown>)
    : {};
}

function asString(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

function asNullableString(value: unknown): string | null {
  return typeof value === 'string' ? value : null;
}

function asNumber(value: unknown, fallback: number): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : fallback;
}
