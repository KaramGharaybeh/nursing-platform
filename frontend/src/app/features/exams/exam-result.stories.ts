import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamFullResult } from '../../core/api/exams-api';
import { ExamResultScreen } from './exam-result';

function passedResult(): ExamFullResult {
  return {
    sessionId: 'session-9',
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    score: 68,
    maxScore: 75,
    percentage: 90.67,
    passed: true,
    correctCount: 68,
    questionCount: 75,
  };
}

class SubmittedPassedApi {
  getExamSessionResult() {
    return of(passedResult());
  }
}

class SubmittedNotPassedApi {
  getExamSessionResult() {
    return of({ ...passedResult(), passed: false, score: 40, percentage: 53.33, correctCount: 40 });
  }
}

class ExpiredApi {
  getExamSessionResult() {
    return of({ ...passedResult(), status: 'Expired', passed: false, score: 40, percentage: 53.33 });
  }
}

class UnavailableApi {
  getExamSessionResult() {
    return throwError(() => ({ status: 404 }));
  }
}

class NotFinalizedApi {
  getExamSessionResult() {
    return throwError(() => ({ status: 409 }));
  }
}

class LoadErrorApi {
  getExamSessionResult() {
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
        snapshot: { paramMap: convertToParamMap({ examId: 'exam-1', sessionId: 'session-9' }) },
      },
    },
  ];
}

const meta: Meta<ExamResultScreen> = {
  component: ExamResultScreen,
  title: 'Features/Exams/ExamResult',
};
export default meta;
type Story = StoryObj<ExamResultScreen>;

export const SubmittedPassed: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new SubmittedPassedApi()) })],
};

export const SubmittedNotPassed: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new SubmittedNotPassedApi()) })],
};

export const Expired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExpiredApi()) })],
};

export const Unavailable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnavailableApi()) })],
};

export const NotFinalized: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NotFinalizedApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};
