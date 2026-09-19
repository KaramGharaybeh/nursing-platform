import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { ExamsApi } from '../../../core/api/exams-api';
import { PackageExamSection } from './package-exam-section';

function rights(available: boolean, status = 'Available') {
  return [
    {
      rightType: 'PackageExamAttemptEligibility',
      status,
      accessStartsAt: '2026-09-01T00:00:00Z',
      accessEndsAt: null,
      isAvailable: available,
      isDormant: false,
    },
  ];
}

function sessionState(state: Record<string, unknown>) {
  return {
    hasSession: false,
    sessionId: null,
    examId: null,
    status: null,
    expiresAt: null,
    ...state,
  };
}

class BaseApi {
  examState: Record<string, unknown> = sessionState({});
  startCalls: string[] = [];

  listMyEntitlements() {
    return throwError(() => ({ status: 500 }));
  }

  getMyEntitlement() {
    return throwError(() => ({ status: 500 }));
  }

  getPackageExamSessionState() {
    return of(this.examState);
  }

  startPackageExamSession(entitlementId: string) {
    this.startCalls.push(entitlementId);
    return of({ sessionId: 'sess-9', examId: 'exam-1' });
  }

  getPackageAnalyticalReport() {
    return throwError(() => ({ status: 500 }));
  }
}

class BaseExamsApi {
  getExamSession() {
    return of({
      id: 'sess-1',
      examId: 'exam-1',
      status: 'InProgress',
      expiresAt: '2999-01-01T00:00:00Z',
      remainingSeconds: 3600,
    });
  }
}

function providers(api: BaseApi) {
  return [
    provideRouter([]),
    { provide: PreparationPackageEntitlementsApi, useValue: api },
    { provide: ExamsApi, useValue: new BaseExamsApi() },
  ];
}

function inputs(active: boolean, attemptRights = rights(true)) {
  return { entitlementId: 'ent-1', rights: attemptRights, entitlementActive: active };
}

const meta: Meta<PackageExamSection> = {
  component: PackageExamSection,
  title: 'Features/NursePreparationPackages/PackageExamSection',
  args: { entitlementId: 'ent-1', rights: rights(true), entitlementActive: true },
};
export default meta;
type Story = StoryObj<PackageExamSection>;

export const Start: Story = {
  decorators: [
    (story) => ({ ...story(), providers: providers(new BaseApi()), props: { ...inputs(true) } }),
  ],
};

export const Resume: Story = {
  decorators: [
    (story) => {
      const api = new BaseApi();
      api.examState = sessionState({
        hasSession: true,
        sessionId: 'sess-1',
        examId: 'exam-1',
        status: 'InProgress',
        expiresAt: '2999-01-01T00:00:00Z',
      });
      return { ...story(), providers: providers(api), props: { ...inputs(true) } };
    },
  ],
};

export const Completed: Story = {
  decorators: [
    (story) => {
      const api = new BaseApi();
      api.examState = sessionState({
        hasSession: true,
        sessionId: 'sess-1',
        examId: 'exam-1',
        status: 'Submitted',
        expiresAt: '2026-09-01T00:00:00Z',
      });
      return { ...story(), providers: providers(api), props: { ...inputs(true) } };
    },
  ],
};

export const AttemptUsed: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: providers(new BaseApi()),
      props: { ...inputs(true, rights(false, 'Consumed')) },
    }),
  ],
};

export const PackageExpired: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: providers(new BaseApi()),
      props: { ...inputs(false) },
    }),
  ],
};
