import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { PreparationPackageEntitlementsApi } from '../../../core/api/preparation-package-entitlements-api';
import type { PackageAnalyticalReportDto } from '../../../core/api/generated/models/package-analytical-report-dto';
import { PackageReport } from './package-report';

function report(overrides: Partial<PackageAnalyticalReportDto> = {}): PackageAnalyticalReportDto {
  return {
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
      {
        reportingTopicId: 'topic-2',
        topicName: 'Pharmacology',
        topicDescription: null,
        scoredQuestionCount: 1,
        correctCount: 0,
        earnedPoints: 0,
        availablePoints: 1,
        percentage: 0,
        sortOrder: 2,
      },
    ],
    guidanceItems: [
      {
        reportingTopicId: 'topic-2',
        sourceType: 'StudyMaterialVersion',
        sourceVersionId: 'ver-1',
        title: 'Cardiac Meds Review',
        sourceMetadata: null,
        sortOrder: 1,
      },
      {
        reportingTopicId: 'topic-2',
        sourceType: 'PracticeCollectionVersion',
        sourceVersionId: 'col-1',
        title: 'Pharma Practice Set',
        sourceMetadata: null,
        sortOrder: 2,
      },
    ],
    ...overrides,
  };
}

class EntitlementsApiStub {
  current: PackageAnalyticalReportDto = report();
  reportError: unknown = undefined;
  requested: string[] = [];

  listMyEntitlements() {
    return throwError(() => ({ status: 500 }));
  }

  getMyEntitlement() {
    return throwError(() => ({ status: 500 }));
  }

  getPackageExamSessionState() {
    return throwError(() => ({ status: 500 }));
  }

  startPackageExamSession() {
    return throwError(() => ({ status: 500 }));
  }

  getPackageAnalyticalReport(sessionId: string) {
    this.requested.push(sessionId);
    if (this.reportError !== undefined) {
      return throwError(() => this.reportError);
    }
    return of(this.current);
  }
}

async function setup(stub?: EntitlementsApiStub): Promise<{ fixture: ComponentFixture<PackageReport>; api: EntitlementsApiStub }> {
  const api = stub ?? new EntitlementsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [PackageReport],
    providers: [
      provideRouter([]),
      { provide: PreparationPackageEntitlementsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ sessionId: 'sess-1' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(PackageReport);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<PackageReport>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<PackageReport>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<PackageReport>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('PackageReport (T-FE-079)', () => {
  it('loads by sessionId and renders the approved summary facts', async () => {
    const { fixture, api } = await setup();
    const content = text(fixture);

    expect(api.requested).toEqual(['sess-1']);
    expect(content).toContain('Exam report');
    expect(byTestId(fixture, 'package-report-correct')?.textContent).toContain('1');
    expect(byTestId(fixture, 'package-report-questions')?.textContent).toContain('2');
    expect(byTestId(fixture, 'package-report-percentage')?.textContent).toContain('50');
    expect(content).not.toMatch(/Strong|Weak|Needs improvement|grade/i);
    expect(content).not.toMatch(/Passed|Not passed|Failed/i);
  });

  it('renders topics in backend order without performance labels', async () => {
    const { fixture } = await setup();
    const element = fixture.nativeElement as HTMLElement;
    const names = [...element.querySelectorAll('[data-testid="package-report-topic"]')].map((row) =>
      row.querySelector('[data-testid="package-report-topic-name"]')?.textContent?.trim(),
    );

    expect(names).toEqual(['Cardiology', 'Pharmacology']);
    expect(text(fixture)).toContain('100');
    expect(text(fixture)).not.toMatch(/Strong|Weak|Needs improvement/i);
  });

  it('renders guidance with approved type labels in backend order', async () => {
    const { fixture } = await setup();
    const element = fixture.nativeElement as HTMLElement;
    const entries = [...element.querySelectorAll('[data-testid="package-report-guidance"]')].map((row) => [
      row.querySelectorAll('p')[0]?.textContent?.trim() ?? '',
      row.querySelectorAll('p')[1]?.textContent?.trim() ?? '',
    ]);

    expect(entries).toEqual([
      ['Study material', 'Cardiac Meds Review'],
      ['Practice collection', 'Pharma Practice Set'],
    ]);
  });

  it('shows the approved empty-guidance copy when no guidance exists', async () => {
    const stub = new EntitlementsApiStub();
    stub.current = report({ guidanceItems: [] });
    const { fixture } = await setup(stub);

    expect(text(fixture)).toContain(
      'No additional package content recommendations are available for this report.',
    );
  });

  it('exposes no review, answer, key, or identifier internals', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).not.toMatch(UUID_PATTERN);
    expect(content).not.toMatch(/question text|correct answer|answer key|rationale|explanation|provenance|PackageBenefitRightId/i);
  });

  it('shows the approved unavailable copy on 404', async () => {
    const stub = new EntitlementsApiStub();
    stub.reportError = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'package-report-unavailable')?.textContent).toContain(
      "This exam report isn't available.",
    );
  });

  it('shows the approved not-finalized copy on 409', async () => {
    const stub = new EntitlementsApiStub();
    stub.reportError = { status: 409 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'package-report-unavailable')?.textContent).toContain(
      'Finish the exam before viewing this report.',
    );
  });

  it('retries a generic failure with the same session id', async () => {
    const stub = new EntitlementsApiStub();
    stub.reportError = { status: 500 };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    expect(text(fixture)).toContain("We couldn't load this exam report. Try again.");

    stub.reportError = undefined;
    await component.retry();
    await settle(fixture);

    expect(api.requested).toEqual(['sess-1', 'sess-1']);
    expect(byTestId(fixture, 'package-report-correct')).not.toBeNull();
  });

  it('provides a stable back link to preparation packages after direct load', async () => {
    const { fixture } = await setup();
    const link = byTestId(fixture, 'package-report-back-link') as HTMLAnchorElement | null;

    expect(link).not.toBeNull();
    expect(link?.getAttribute('href')).toBe('/nurse/preparation-packages');
  });
});

describe('PackageReport route', () => {
  it('mounts the canonical report route with guards and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/preparation-packages/reports/:sessionId');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_REPORT' });
  });
});
