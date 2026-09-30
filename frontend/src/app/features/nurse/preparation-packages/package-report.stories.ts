import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { PackageReport } from './package-report';

const POPULATED = {
  id: 'rep-1',
  examSessionId: 'sess-1',
  generatedAt: '2026-09-18T01:00:00Z',
  finalizedSessionStatus: 1,
  submittedAt: '2026-09-18T00:30:00Z',
  finalizedAt: '2026-09-18T00:30:00Z',
  score: 1,
  maxScore: 2,
  percentage: 50,
  passed: false,
  correctCount: 1,
  questionCount: 2,
  topicResults: [
    {
      reportingTopicId: 'topic-1',
      topicName: 'Cardiology',
      topicDescription: null,
      scoredQuestionCount: 1,
      correctCount: 1,
      earnedPoints: 1,
      availablePoints: 1,
      percentage: 100,
      sortOrder: 1,
    },
  ],
  guidanceItems: [
    {
      reportingTopicId: 'topic-1',
      sourceType: 'StudyMaterialVersion',
      sourceVersionId: 'ver-1',
      title: 'Cardiac Meds Review',
      sourceMetadata: null,
      sortOrder: 1,
    },
  ],
};

class PopulatedApi {
  getPackageAnalyticalReport() {
    return of(POPULATED);
  }
}

class NoGuidanceApi extends PopulatedApi {
  override getPackageAnalyticalReport() {
    return of({ ...POPULATED, guidanceItems: [] });
  }
}

class UnavailableApi extends PopulatedApi {
  override getPackageAnalyticalReport() {
    return throwError(() => ({ status: 404 }));
  }
}

class NotFinalizedApi extends PopulatedApi {
  override getPackageAnalyticalReport() {
    return throwError(() => ({ status: 409 }));
  }
}

class FailingApi extends PopulatedApi {
  override getPackageAnalyticalReport() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: PopulatedApi) {
  return [
    provideRouter([]),
    { provide: PreparationPackageEntitlementsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ sessionId: 'sess-1' }) } },
    },
  ];
}

const meta: Meta<PackageReport> = {
  component: PackageReport,
  title: 'Features/NursePreparationPackages/PackageReport',
};
export default meta;
type Story = StoryObj<PackageReport>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const NoGuidance: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NoGuidanceApi()) })],
};

export const Unavailable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnavailableApi()) })],
};

export const NotFinalized: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NotFinalizedApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new FailingApi()) })],
};
