import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CountriesApi } from '../../core/api/countries-api';
import { ExamsApi } from '../../core/api/exams-api';
import { ExamAnalyticsScreen } from './exam-analytics';

function populatedSummary() {
  return {
    attemptCount: 3,
    submittedCount: 2,
    expiredCount: 1,
    inProgressCount: 0,
    passedCount: 2,
    failedCount: 1,
    passRate: 66.67,
    averageScorePercentage: 80,
    bestScorePercentage: 100,
    latestScorePercentage: 90,
  };
}

function populatedExamPage() {
  return {
    items: [
      {
        examTitle: 'NCLEX Readiness',
        attemptCount: 2,
        passRate: 50,
        averageScorePercentage: 70,
        bestScorePercentage: 90,
        latestScorePercentage: 60,
      },
    ],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
  };
}

function populatedCategoryPage() {
  return {
    items: [
      {
        categoryId: 'cat-1',
        categoryName: 'Licensure',
        attemptCount: 3,
        passRate: 66.67,
        averageScorePercentage: 82,
        bestScorePercentage: 100,
      },
    ],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
  };
}

function populatedTrends() {
  return [
    {
      bucketStart: '2026-01-01T00:00:00Z',
      bucketEnd: '2026-02-01T00:00:00Z',
      attemptCount: 1,
      averageScorePercentage: 90,
      passRate: 100,
    },
  ];
}

class PopulatedApi {
  getExamAnalyticsSummary() {
    return of(populatedSummary());
  }

  listExamAnalyticsByExam() {
    return of(populatedExamPage());
  }

  listExamAnalyticsByCategory() {
    return of(populatedCategoryPage());
  }

  listExamAnalyticsTrends() {
    return of(populatedTrends());
  }
}

class NullableMetricsApi extends PopulatedApi {
  override getExamAnalyticsSummary() {
    return of({ ...populatedSummary(), passRate: null, averageScorePercentage: null });
  }
}

class PageEmptyApi extends PopulatedApi {
  override getExamAnalyticsSummary() {
    return of({ ...populatedSummary(), attemptCount: 0 });
  }
}

class SparseSectionsApi extends PopulatedApi {
  override listExamAnalyticsByExam() {
    return of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  }

  override listExamAnalyticsTrends() {
    return of([]);
  }
}

class SectionErrorApi extends PopulatedApi {
  override listExamAnalyticsByCategory() {
    return throwError(() => ({ status: 500 }));
  }
}

class LoadErrorApi extends PopulatedApi {
  override getExamAnalyticsSummary() {
    return throwError(() => ({ status: 500 }));
  }
}

class Lookups {
  list() {
    return of([{ id: 'c-1', name: 'Jordan', code: 'JO' }]);
  }
}

function providers(api: unknown) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    { provide: CountriesApi, useValue: new Lookups() },
    {
      provide: ActivatedRoute,
      useValue: {
        snapshot: { paramMap: convertToParamMap({}), queryParamMap: convertToParamMap({}) },
      },
    },
    Router,
  ];
}

const meta: Meta<ExamAnalyticsScreen> = {
  component: ExamAnalyticsScreen,
  title: 'Features/Exams/ExamAnalytics',
};
export default meta;
type Story = StoryObj<ExamAnalyticsScreen>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const NullableMetrics: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NullableMetricsApi()) })],
};

export const PageEmpty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PageEmptyApi()) })],
};

export const SparseSections: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new SparseSectionsApi()) })],
};

export const SectionError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new SectionErrorApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};

function filteredProviders(api: unknown) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    { provide: CountriesApi, useValue: new Lookups() },
    {
      provide: ActivatedRoute,
      useValue: {
        snapshot: {
          paramMap: convertToParamMap({}),
          queryParamMap: convertToParamMap({ countryId: 'c-1' }),
        },
      },
    },
    Router,
  ];
}

export const FilteredState: Story = {
  decorators: [(story) => ({ ...story(), providers: filteredProviders(new PopulatedApi()) })],
};
