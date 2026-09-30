import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamCatalogPage } from '../../core/api/exams-api';
import { ExamsList } from './exams-list';

const POPULATED: ExamCatalogPage = {
  items: [
    {
      id: 'exam-1',
      title: 'NCLEX Readiness',
      description: 'Are you ready?',
      countryId: 'country-1',
      countryName: 'Jordan',
      categoryId: 'cat-1',
      categoryName: 'Licensure',
      durationMinutes: 120,
      questionCount: 75,
      passingScorePercentage: 70,
      isFree: true,
      canStart: true,
    },
    {
      id: 'exam-2',
      title: 'Midwifery Basics',
      description: null,
      countryId: 'country-2',
      countryName: 'Egypt',
      categoryId: null,
      categoryName: null,
      durationMinutes: 60,
      questionCount: 40,
      passingScorePercentage: 60,
      isFree: false,
      canStart: true,
    },
  ],
  page: 1,
  pageSize: 20,
  totalCount: 2,
  totalPages: 1,
};

const EMPTY: ExamCatalogPage = { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };

class PopulatedApi {
  listExams() {
    return of(POPULATED);
  }

  getExam() {
    return throwError(() => ({ status: 404 }));
  }

  listCountries() {
    return of([
      { id: 'country-1', name: 'Jordan' },
      { id: 'country-2', name: 'Egypt' },
    ]);
  }
}

class EmptyApi extends PopulatedApi {
  override listExams() {
    return of(EMPTY);
  }
}

class NoResultsApi extends PopulatedApi {
  override listExams() {
    return of(EMPTY);
  }
}

class FailingApi extends PopulatedApi {
  override listExams() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: ExamsApi, useValue: api }];
}

const meta: Meta<ExamsList> = {
  component: ExamsList,
  title: 'Features/Exams/ExamsList',
};
export default meta;
type Story = StoryObj<ExamsList>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const FilteredNoResults: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NoResultsApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new FailingApi()) })],
};
