import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamDetail } from '../../core/api/exams-api';
import { ExamInstructions } from './exam-instructions';

const EXAM: ExamDetail = {
  id: 'exam-1',
  title: 'NCLEX Readiness',
  description: 'Are you ready?',
  instructions: 'Answer every question carefully.',
  countryId: 'country-1',
  countryName: 'Jordan',
  categoryId: 'cat-1',
  categoryName: 'Licensure',
  durationMinutes: 120,
  questionCount: 75,
  passingScorePercentage: 70,
  isFree: true,
  canStart: true,
};

const RESUMABLE = {
  id: 'sess-1',
  examId: 'exam-1',
  examTitle: 'NCLEX Readiness',
  status: 'InProgress',
  startedAt: '2026-09-18T00:00:00Z',
  expiresAt: '2999-01-01T00:00:00Z',
  finalizedAt: null,
  score: null,
  maxScore: null,
  percentage: null,
  passed: null,
};

class StartApi {
  getExam() {
    return of(EXAM);
  }

  findResumableAttempt() {
    return Promise.resolve(undefined);
  }

  startExamSession() {
    return of({ sessionId: 'session-9', examId: 'exam-1' });
  }
}

class ResumeApi extends StartApi {
  override findResumableAttempt() {
    return Promise.resolve(RESUMABLE);
  }
}

class MissingInstructionsApi extends StartApi {
  override getExam() {
    return of({ ...EXAM, instructions: null });
  }
}

class PurchaseRequiredApi extends StartApi {
  override getExam() {
    return of({ ...EXAM, isFree: false, canStart: false });
  }
}

class ResumeLoadingApi extends StartApi {
  override findResumableAttempt(): Promise<undefined> {
    return new Promise(() => undefined);
  }
}

class ResumeErrorApi extends StartApi {
  override findResumableAttempt() {
    return Promise.reject({ status: 500 });
  }
}

class MissingApi extends StartApi {
  override getExam() {
    return throwError(() => ({ status: 404 }));
  }

  override findResumableAttempt() {
    return Promise.reject({ status: 404 });
  }
}

function providers(api: StartApi) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } },
    },
  ];
}

const meta: Meta<ExamInstructions> = {
  component: ExamInstructions,
  title: 'Features/Exams/ExamInstructions',
};
export default meta;
type Story = StoryObj<ExamInstructions>;

export const Start: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new StartApi()) })],
};

export const Resume: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ResumeApi()) })],
};

export const MissingInstructions: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingInstructionsApi()) })],
};

export const PurchaseRequired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PurchaseRequiredApi()) })],
};

export const ResumeLoading: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ResumeLoadingApi()) })],
};

export const ResumeError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ResumeErrorApi()) })],
};

export const Missing: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingApi()) })],
};
