import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamDetail as ExamDetailModel } from '../../core/api/exams-api';
import { ExamDetail } from './exam-detail';

const STARTABLE: ExamDetailModel = {
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

const PAID_LOCKED: ExamDetailModel = {
  ...STARTABLE,
  id: 'exam-2',
  isFree: false,
  canStart: false,
};

class StartableApi {
  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    return of(STARTABLE);
  }

  listCountries() {
    return of([]);
  }
}

class PaidLockedApi extends StartableApi {
  override getExam() {
    return of(PAID_LOCKED);
  }
}

class MissingApi extends StartableApi {
  override getExam() {
    return throwError(() => ({ status: 404 }));
  }
}

function providers(api: StartableApi) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } },
    },
  ];
}

const meta: Meta<ExamDetail> = {
  component: ExamDetail,
  title: 'Features/Exams/ExamDetail',
};
export default meta;
type Story = StoryObj<ExamDetail>;

export const Startable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new StartableApi()) })],
};

export const PurchaseRequired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PaidLockedApi()) })],
};

export const Missing: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingApi()) })],
};
