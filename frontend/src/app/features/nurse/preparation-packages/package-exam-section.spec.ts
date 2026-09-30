import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import { ExamsApi } from '../../../core/api/exams-api';
import type { PackageBenefitRightSummaryDto } from '../../../core/api/generated/models/package-benefit-right-summary-dto';
import { PackageExamSection } from './package-exam-section';

function right(type: string, available: boolean, status = 'Available'): PackageBenefitRightSummaryDto {
  return {
    rightType: type,
    status,
    accessStartsAt: '2026-09-01T00:00:00Z',
    accessEndsAt: null,
    isAvailable: available,
    isDormant: false,
  };
}

const ATTEMPT = () => right('PackageExamAttemptEligibility', true);

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

class EntitlementsApiStub {
  examState: Record<string, unknown> = sessionState({});
  examStateError: unknown = undefined;
  examStateCalls: string[] = [];
  startCalls: string[] = [];
  startResult = { sessionId: 'sess-9', examId: 'exam-1' };
  startError: unknown = undefined;

  listMyEntitlements() {
    return throwError(() => ({ status: 500 }));
  }

  getMyEntitlement() {
    return throwError(() => ({ status: 500 }));
  }

  getPackageExamSessionState(entitlementId: string) {
    this.examStateCalls.push(entitlementId);
    if (this.examStateError !== undefined) {
      return throwError(() => this.examStateError);
    }
    return of(this.examState);
  }

  startPackageExamSession(entitlementId: string) {
    this.startCalls.push(entitlementId);
    if (this.startError !== undefined) {
      return throwError(() => this.startError);
    }
    return of(this.startResult);
  }

  getPackageAnalyticalReport() {
    return throwError(() => ({ status: 500 }));
  }
}

async function setup(
  stub?: EntitlementsApiStub,
  inputs: { rights?: PackageBenefitRightSummaryDto[]; entitlementActive?: boolean } = {},
  examsStub?: { getExamSession(sessionId: string): unknown },
): Promise<{ fixture: ComponentFixture<PackageExamSection>; api: EntitlementsApiStub; navigateSpy: Mock }> {
  const api = stub ?? new EntitlementsApiStub();
  const exams = examsStub ?? {
    getExamSession: () =>
      of({
        id: 'sess-1',
        examId: 'exam-1',
        status: 'InProgress',
        expiresAt: '2999-01-01T00:00:00Z',
        remainingSeconds: 3600,
      }),
  };
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [PackageExamSection],
    providers: [
      provideRouter([]),
      { provide: PreparationPackageEntitlementsApi, useValue: api },
      { provide: ExamsApi, useValue: exams },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(PackageExamSection);
  fixture.componentRef.setInput('entitlementId', 'ent-1');
  fixture.componentRef.setInput('rights', inputs.rights ?? [ATTEMPT()]);
  fixture.componentRef.setInput('entitlementActive', inputs.entitlementActive ?? true);
  const router = TestBed.inject(Router);
  const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, navigateSpy };
}

async function settle(fixture: ComponentFixture<PackageExamSection>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<PackageExamSection>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<PackageExamSection>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('PackageExamSection (T-FE-079)', () => {
  it('renders a Package exam section with Start exam when eligible and fires no POST on load', async () => {
    const { fixture, api } = await setup();

    expect(byTestId(fixture, 'package-exam-heading')?.textContent).toContain('Package exam');
    expect(byTestId(fixture, 'package-exam-start')?.textContent).toContain('Start exam');
    expect(api.startCalls).toEqual([]);
    expect(api.examStateCalls).toEqual(['ent-1']);
  });

  it('shows Resume exam for an unexpired package InProgress session', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-1',
      examId: 'exam-1',
      status: 'InProgress',
      expiresAt: '2999-01-01T00:00:00Z',
    });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'package-exam-resume')?.textContent).toContain('Resume exam');
    expect(byTestId(fixture, 'package-exam-start')).toBeNull();
  });

  it('shows Exam completed with a report link for a Submitted session', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-1',
      examId: 'exam-1',
      status: 'Submitted',
      expiresAt: '2026-09-01T00:00:00Z',
    });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'package-exam-completed')).not.toBeNull();
    const link = byTestId(fixture, 'package-exam-report-link') as HTMLAnchorElement | null;
    expect(link?.getAttribute('href')).toBe('/nurse/preparation-packages/reports/sess-1');
    expect(byTestId(fixture, 'package-exam-start')).toBeNull();
    expect(byTestId(fixture, 'package-exam-resume')).toBeNull();
  });

  it('shows Exam expired with a report link for an Expired session', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-2',
      examId: 'exam-1',
      status: 'Expired',
      expiresAt: '2026-09-01T00:00:00Z',
    });
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'package-exam-expired')).not.toBeNull();
    expect(
      (byTestId(fixture, 'package-exam-report-link') as HTMLAnchorElement).getAttribute('href'),
    ).toBe('/nurse/preparation-packages/reports/sess-2');
  });

  it('shows Exam attempt used when consumed with no safe session', async () => {
    const { fixture } = await setup(new EntitlementsApiStub(), {
      rights: [right('PackageExamAttemptEligibility', false, 'Consumed')],
    });

    expect(byTestId(fixture, 'package-exam-used')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-start')).toBeNull();
    expect(byTestId(fixture, 'package-exam-resume')).toBeNull();
    expect(byTestId(fixture, 'package-exam-report-link')).toBeNull();
  });

  it('shows Package expired for an expired entitlement with no session', async () => {
    const { fixture } = await setup(new EntitlementsApiStub(), { entitlementActive: false });

    expect(byTestId(fixture, 'package-exam-package-expired')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-start')).toBeNull();
    expect(byTestId(fixture, 'package-exam-resume')).toBeNull();
  });

  it('prefers a finalized package session over entitlement expiry', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-9',
      examId: 'exam-1',
      status: 'Submitted',
      expiresAt: '2026-09-01T00:00:00Z',
    });
    const { fixture } = await setup(stub, { entitlementActive: false });

    expect(byTestId(fixture, 'package-exam-completed')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-report-link')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-package-expired')).toBeNull();
  });

  it('resolves state exclusively through the package session-state operation', async () => {
    const { fixture, api } = await setup();

    expect(api.examStateCalls).toEqual(['ent-1']);
    expect(api.startCalls).toEqual([]);
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
  });

  it('reconciles a stale InProgress presentation through GetExamSession', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-1',
      examId: 'exam-1',
      status: 'InProgress',
      expiresAt: '2020-01-01T00:00:00Z',
    });
    const { fixture } = await setup(
      stub,
      {},
      {
        getExamSession: () =>
          of({
            id: 'sess-1',
            examId: 'exam-1',
            status: 'Expired',
            expiresAt: '2020-01-01T00:00:00Z',
            remainingSeconds: 0,
          }),
      },
    );

    expect(byTestId(fixture, 'package-exam-start')).toBeNull();
    expect(byTestId(fixture, 'package-exam-resume')).toBeNull();
    expect(byTestId(fixture, 'package-exam-expired')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-report-link')).not.toBeNull();
  });

  it('shows a retryable state when package-state load fails instead of Start', async () => {
    const stub = new EntitlementsApiStub();
    stub.examStateError = { status: 500 };
    const { fixture, api } = await setup(stub);

    expect(byTestId(fixture, 'package-exam-state-error')).not.toBeNull();
    expect(byTestId(fixture, 'package-exam-start')).toBeNull();

    api.examStateError = undefined;
    (byTestId(fixture, 'package-exam-state-retry') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.examStateCalls).toEqual(['ent-1', 'ent-1']);
    expect(byTestId(fixture, 'package-exam-start')).not.toBeNull();
  });

  it('Start confirmation uses exact copy and Cancel sends zero POST', async () => {
    const { fixture, api } = await setup();

    (byTestId(fixture, 'package-exam-start') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(byTestId(fixture, 'package-exam-confirm-title')?.textContent).toContain('Start exam?');
    expect(text(fixture)).toContain('This package includes one exam attempt.');

    (byTestId(fixture, 'package-exam-cancel') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual([]);
    expect(byTestId(fixture, 'package-exam-confirm-title')).toBeNull();
  });

  it('confirming Start posts the exact entitlement id once and navigates from the response', async () => {
    const { fixture, api, navigateSpy } = await setup();

    (byTestId(fixture, 'package-exam-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'package-exam-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual(['ent-1']);
    expect(navigateSpy).toHaveBeenCalledWith('/exams/exam-1/sessions/sess-9');
  });

  it('Resume confirmation uses resume copy and the same POST operation', async () => {
    const stub = new EntitlementsApiStub();
    stub.examState = sessionState({
      hasSession: true,
      sessionId: 'sess-1',
      examId: 'exam-1',
      status: 'InProgress',
      expiresAt: '2999-01-01T00:00:00Z',
    });
    stub.startResult = { sessionId: 'sess-1', examId: 'exam-1' };
    const { fixture, api, navigateSpy } = await setup(stub);

    (byTestId(fixture, 'package-exam-resume') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(text(fixture)).toContain('Resume your existing timed exam session.');
    expect(text(fixture)).not.toContain('uses that attempt');

    (byTestId(fixture, 'package-exam-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual(['ent-1']);
    expect(navigateSpy).toHaveBeenCalledWith('/exams/exam-1/sessions/sess-1');
  });

  it('reconciles a 409 without a blind second POST', async () => {
    const stub = new EntitlementsApiStub();
    stub.startError = { status: 409 };
    const { fixture, api } = await setup(stub);

    (byTestId(fixture, 'package-exam-start') as HTMLButtonElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'package-exam-confirm-go') as HTMLButtonElement).click();
    fixture.detectChanges();
    await settle(fixture);

    expect(api.startCalls).toEqual(['ent-1']);
    expect(api.examStateCalls.length).toBeGreaterThan(1);
    expect(byTestId(fixture, 'package-exam-start-error')).not.toBeNull();
    expect(text(fixture)).not.toMatch(/409|consumed-|conflict/i);
  });
});
