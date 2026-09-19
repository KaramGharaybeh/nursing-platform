import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getExam } from './generated/fn/nursing-platform-web-api/get-exam';
import { getExamSession } from './generated/fn/nursing-platform-web-api/get-exam-session';
import { getExamSessionResult } from './generated/fn/nursing-platform-web-api/get-exam-session-result';
import { getExamSessionReview } from './generated/fn/nursing-platform-web-api/get-exam-session-review';
import { listCountries } from './generated/fn/nursing-platform-web-api/list-countries';
import { listExams } from './generated/fn/nursing-platform-web-api/list-exams';
import { listMyExamAttempts } from './generated/fn/nursing-platform-web-api/list-my-exam-attempts';
import { saveExamSessionAnswers } from './generated/fn/nursing-platform-web-api/save-exam-session-answers';
import { startExamSession } from './generated/fn/nursing-platform-web-api/start-exam-session';
import { submitExamSession } from './generated/fn/nursing-platform-web-api/submit-exam-session';
import type { ExamAttemptDto } from './generated/models/exam-attempt-dto';

const ATTEMPTS_PAGE_SIZE = 100;
const SESSION_STATUS_IN_PROGRESS = 0;

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

export interface ExamSessionStart {
  readonly sessionId: string;
  readonly examId: string;
}

export interface ExamSessionQuestion {
  readonly examSessionQuestionId: string;
  readonly text: string;
  readonly points: number;
  readonly displayOrder: number;
  readonly selectedExamSessionAnswerOptionId: string | null;
  readonly options: ExamSessionAnswerOption[];
}

export interface ExamSessionAnswerOption {
  readonly examSessionAnswerOptionId: string;
  readonly text: string;
  readonly displayOrder: number;
}

export interface ExamSession {
  readonly id: string;
  readonly examId: string;
  readonly examTitle: string;
  readonly status: string;
  readonly startedAt: string;
  readonly expiresAt: string;
  readonly remainingSeconds: number;
  readonly items: ExamSessionQuestion[];
}

export interface ExamSessionAnswer {
  readonly examSessionQuestionId: string;
  readonly selectedExamSessionAnswerOptionId: string;
}

export interface ExamSessionResult {
  readonly score: number;
  readonly maxScore: number;
  readonly percentage: number;
  readonly passed: boolean;
  readonly correctCount: number;
  readonly questionCount: number;
}

export interface ExamFullResult extends ExamSessionResult {
  readonly sessionId: string;
  readonly examId: string;
  readonly examTitle: string | null;
  readonly status: string;
}

export interface ExamReviewOption {
  readonly displayOrder: number;
  readonly text: string;
  readonly isCorrect: boolean;
  readonly isSelected: boolean;
}

export interface ExamReviewQuestion {
  readonly displayOrder: number;
  readonly text: string;
  readonly explanation: string | null;
  readonly points: number;
  readonly pointsEarned: number;
  readonly options: ExamReviewOption[];
}

export interface ExamReview {
  readonly examId: string;
  readonly examTitle: string | null;
  readonly status: string;
  readonly items: ExamReviewQuestion[];
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

  listMyExamAttempts(query: { status: number; page: number; pageSize: number }): Observable<{
    readonly items: ExamAttemptDto[];
    readonly page: number;
    readonly pageSize: number;
    readonly totalCount: number;
    readonly totalPages: number;
  }> {
    return listMyExamAttempts(this.http, this.config.rootUrl, {
      status: query.status,
      page: query.page,
      pageSize: query.pageSize,
    }).pipe(map((response) => response.body));
  }

  async findResumableAttempt(examId: string, now: Date = new Date()): Promise<ExamAttemptDto | undefined> {
    let page = 1;
    for (;;) {
      const result = await firstValueFrom(
        this.listMyExamAttempts({ status: SESSION_STATUS_IN_PROGRESS, page, pageSize: ATTEMPTS_PAGE_SIZE }),
      );
      const match = result.items.find((attempt) => isResumableMatch(attempt, examId, now));
      if (match !== undefined) {
        return match;
      }
      if (result.page >= result.totalPages || result.items.length === 0) {
        return undefined;
      }
      page += 1;
    }
  }

  startExamSession(examId: string): Observable<ExamSessionStart> {
    return startExamSession(this.http, this.config.rootUrl, { id: examId }).pipe(
      map((response) => adaptSessionStart(response.body)),
    );
  }

  getExamSession(sessionId: string): Observable<ExamSession> {
    return getExamSession(this.http, this.config.rootUrl, { id: sessionId }).pipe(
      map((response) => adaptExamSession(response.body)),
    );
  }

  saveExamSessionAnswers(sessionId: string, answers: ExamSessionAnswer[]): Observable<ExamSession> {
    return saveExamSessionAnswers(this.http, this.config.rootUrl, {
      id: sessionId,
      body: { answers },
    }).pipe(map((response) => adaptExamSession(response.body)));
  }

  submitExamSession(sessionId: string): Observable<ExamSessionResult> {
    return submitExamSession(this.http, this.config.rootUrl, { id: sessionId }).pipe(
      map((response) => adaptExamSessionResult(response.body)),
    );
  }

  getExamSessionResult(sessionId: string): Observable<ExamFullResult> {
    return getExamSessionResult(this.http, this.config.rootUrl, { id: sessionId }).pipe(
      map((response) => adaptExamFullResult(response.body)),
    );
  }

  getExamSessionReview(sessionId: string): Observable<ExamReview> {
    return getExamSessionReview(this.http, this.config.rootUrl, { id: sessionId }).pipe(
      map((response) => adaptExamReview(response.body)),
    );
  }
}

function isResumableMatch(attempt: ExamAttemptDto, examId: string, now: Date): boolean {
  if (attempt.examId !== examId || attempt.status !== 'InProgress') {
    return false;
  }
  const expiresAt = Date.parse(attempt.expiresAt);
  return Number.isFinite(expiresAt) && expiresAt > now.getTime();
}

function adaptSessionStart(body: unknown): ExamSessionStart {
  const record = asRecord(body);
  const sessionId = asString(record['id']);
  const examId = asString(record['examId']);
  if (sessionId === '' || examId === '') {
    throw new Error('Exam session response did not include a session identity.');
  }
  return { sessionId, examId };
}

function adaptExamSession(body: unknown): ExamSession {
  const record = asRecord(body);
  const items = Array.isArray(record['items']) ? record['items'].map(adaptSessionQuestion) : [];
  return {
    id: asString(record['id']),
    examId: asString(record['examId']),
    examTitle: asString(record['examTitle']),
    status: asString(record['status']),
    startedAt: asString(record['startedAt']),
    expiresAt: asString(record['expiresAt']),
    remainingSeconds: asNumber(record['remainingSeconds'], 0),
    items,
  };
}

function adaptSessionQuestion(item: unknown): ExamSessionQuestion {
  const record = asRecord(item);
  const options = Array.isArray(record['options']) ? record['options'].map(adaptSessionOption) : [];
  return {
    examSessionQuestionId: asString(record['id']),
    text: asString(record['text']),
    points: asNumber(record['points'], 0),
    displayOrder: asNumber(record['displayOrder'], 0),
    selectedExamSessionAnswerOptionId: asNullableString(record['selectedExamSessionAnswerOptionId']),
    options,
  };
}

function adaptSessionOption(option: unknown): ExamSessionAnswerOption {
  const record = asRecord(option);
  return {
    examSessionAnswerOptionId: asString(record['id']),
    text: asString(record['text']),
    displayOrder: asNumber(record['displayOrder'], 0),
  };
}

function adaptExamFullResult(body: unknown): ExamFullResult {
  const record = asRecord(body);
  const examTitle = asNullableString(record['examTitle']);
  return {
    sessionId: asString(record['id']),
    examId: asString(record['examId']),
    examTitle: examTitle === null || examTitle.trim() === '' ? null : examTitle,
    status: asString(record['status']),
    ...adaptExamSessionResult(body),
  };
}

function adaptExamReview(body: unknown): ExamReview {
  const record = asRecord(body);
  const examId = asString(record['examId']);
  const examTitle = asNullableString(record['examTitle']);
  const items = record['items'];
  if (examId === '' || !Array.isArray(items)) {
    throw new Error('Exam review response did not include usable review content.');
  }
  return {
    examId,
    examTitle: examTitle === null || examTitle.trim() === '' ? null : examTitle,
    status: asString(record['status']),
    items: items.map(adaptReviewQuestion),
  };
}

function adaptReviewQuestion(item: unknown): ExamReviewQuestion {
  const record = asRecord(item);
  const text = asString(record['text']);
  const rawOptions = record['options'];
  if (text.trim() === '' || !Array.isArray(rawOptions) || rawOptions.length === 0) {
    throw new Error('Exam review response did not include usable review content.');
  }
  const selectedId = asNullableString(record['selectedExamSessionAnswerOptionId']);
  if (selectedId !== null && !rawOptions.some((option) => asString(asRecord(option)['id']) === selectedId)) {
    throw new Error('Exam review response did not include usable review content.');
  }
  const explanation = asNullableString(record['explanation']);
  return {
    displayOrder: asNumber(record['displayOrder'], 0),
    text,
    explanation: explanation === null || explanation.trim() === '' ? null : explanation,
    points: asNumber(record['points'], 0),
    pointsEarned: asNumber(record['pointsEarned'], 0),
    options: rawOptions.map((option) => adaptReviewOption(option, selectedId)),
  };
}

function adaptReviewOption(option: unknown, selectedId: string | null): ExamReviewOption {
  const record = asRecord(option);
  const text = asString(record['text']);
  if (text.trim() === '') {
    throw new Error('Exam review response did not include usable review content.');
  }
  const optionId = asString(record['id']);
  return {
    displayOrder: asNumber(record['displayOrder'], 0),
    text,
    isCorrect: record['isCorrect'] === true,
    isSelected: selectedId !== null && optionId === selectedId,
  };
}

function adaptExamSessionResult(body: unknown): ExamSessionResult {
  const record = asRecord(body);
  return {
    score: asNumber(record['score'], 0),
    maxScore: asNumber(record['maxScore'], 0),
    percentage: asNumber(record['percentage'], 0),
    passed: record['passed'] === true,
    correctCount: asNumber(record['correctCount'], 0),
    questionCount: asNumber(record['questionCount'], 0),
  };
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
