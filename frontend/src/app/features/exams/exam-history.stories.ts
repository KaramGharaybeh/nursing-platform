import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import { ExamHistoryScreen } from './exam-history';

function mixedPage() {
  return {
    items: [
      {
        sessionId: 'session-7',
        examId: 'exam-1',
        examTitle: 'NCLEX Readiness',
        status: 'InProgress',
        startedAt: '2026-09-18T00:00:00Z',
        expiresAt: '2026-09-18T01:00:00Z',
        score: null,
        maxScore: null,
        percentage: null,
        passed: null,
      },
      {
        sessionId: 'session-9',
        examId: 'exam-1',
        examTitle: 'NCLEX Readiness',
        status: 'Submitted',
        startedAt: '2026-09-17T00:00:00Z',
        expiresAt: '2026-09-17T01:00:00Z',
        score: 68,
        maxScore: 75,
        percentage: 90.67,
        passed: true,
      },
      {
        sessionId: 'session-8',
        examId: 'exam-2',
        examTitle: 'Second exam',
        status: 'Expired',
        startedAt: '2026-09-16T00:00:00Z',
        expiresAt: '2026-09-16T01:00:00Z',
        score: 40,
        maxScore: 75,
        percentage: 53.33,
        passed: false,
      },
    ],
    page: 1,
    pageSize: 20,
    totalCount: 3,
    totalPages: 1,
  };
}

class MixedApi {
  listExamHistory() {
    return of(mixedPage());
  }
}

class InProgressApi {
  listExamHistory() {
    return of({ ...mixedPage(), items: [mixedPage().items[0]], totalCount: 1 });
  }
}

class CompletedPassedApi {
  listExamHistory() {
    return of({ ...mixedPage(), items: [mixedPage().items[1]], totalCount: 1 });
  }
}

class CompletedNotPassedApi {
  listExamHistory() {
    return of({
      ...mixedPage(),
      items: [{ ...mixedPage().items[1], passed: false, percentage: 40 }],
      totalCount: 1,
    });
  }
}

class TimeExpiredApi {
  listExamHistory() {
    return of({ ...mixedPage(), items: [mixedPage().items[2]], totalCount: 1 });
  }
}

class EmptyApi {
  listExamHistory() {
    return of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  }
}

class LoadErrorApi {
  listExamHistory() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: unknown) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: {
        snapshot: { paramMap: convertToParamMap({}), queryParamMap: convertToParamMap({}) },
      },
    },
    Router,
  ];
}

const meta: Meta<ExamHistoryScreen> = {
  component: ExamHistoryScreen,
  title: 'Features/Exams/ExamHistory',
};
export default meta;
type Story = StoryObj<ExamHistoryScreen>;

export const MixedHistory: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MixedApi()) })],
};

export const InProgress: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new InProgressApi()) })],
};

export const CompletedPassed: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new CompletedPassedApi()) })],
};

export const CompletedNotPassed: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new CompletedNotPassedApi()) })],
};

export const TimeExpired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new TimeExpiredApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const FilteredEmpty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};
