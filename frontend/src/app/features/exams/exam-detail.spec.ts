import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamDetail as ExamDetailModel } from '../../core/api/exams-api';
import { ExamDetail } from './exam-detail';

const STARTABLE: ExamDetailModel = {
  id: '11111111-1111-4111-8111-111111111111',
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
  id: '22222222-2222-4222-8222-222222222222',
  isFree: false,
  canStart: false,
};

class ExamsApiStub {
  detail: ExamDetailModel = STARTABLE;
  detailError: unknown = undefined;

  listExams() {
    return throwError(() => ({ status: 500 }));
  }

  getExam() {
    if (this.detailError !== undefined) {
      return throwError(() => this.detailError);
    }
    return of(this.detail);
  }

  listCountries() {
    return of([]);
  }
}

async function setup(stub?: ExamsApiStub): Promise<{ fixture: ComponentFixture<ExamDetail>; api: ExamsApiStub }> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamDetail],
    providers: [
      provideRouter([]),
      { provide: ExamsApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamDetail);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<ExamDetail>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamDetail>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<ExamDetail>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamDetail (T-FE-067)', () => {
  it('loads by examId and renders approved metadata including passing score', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('NCLEX Readiness');
    expect(content).toContain('Are you ready?');
    expect(content).toContain('Jordan');
    expect(content).toContain('Licensure');
    expect(content).toContain('120 minutes');
    expect(content).toContain('75 questions');
    expect(content).toContain('70');
    expect(content).not.toMatch(UUID_PATTERN);
  });

  it('shows View instructions for a startable exam using the canonical route', async () => {
    const { fixture } = await setup();
    const link = byTestId(fixture, 'exam-instructions-link') as HTMLAnchorElement | null;

    expect(link).not.toBeNull();
    expect(link?.textContent).toContain('View instructions');
    expect(link?.getAttribute('href')).toBe('/exams/exam-1/instructions');
    expect(byTestId(fixture, 'exam-requires-purchase')).toBeNull();
  });

  it('shows Requires purchase without any purchase CTA for a paid inaccessible exam', async () => {
    const stub = new ExamsApiStub();
    stub.detail = PAID_LOCKED;
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(byTestId(fixture, 'exam-requires-purchase')?.textContent).toContain('Requires purchase');
    expect(byTestId(fixture, 'exam-instructions-link')).toBeNull();
    const actions = [...(fixture.nativeElement as HTMLElement).querySelectorAll('a, button')].filter(
      (element) => element.getAttribute('data-testid') !== 'exam-back-link' && element.getAttribute('data-testid') !== 'exam-bottom-back-link',
    );
    expect(actions).toEqual([]);
    expect(content).not.toMatch(/checkout|buy now|pay now|price|cart/i);
  });

  it('shows a contextual missing notice with a back link on 404', async () => {
    const stub = new ExamsApiStub();
    stub.detailError = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'exam-missing-notice')).not.toBeNull();
    expect(byTestId(fixture, 'exam-back-link')).not.toBeNull();
  });

  it('retries a backend error', async () => {
    const stub = new ExamsApiStub();
    stub.detailError = { status: 500 };
    const { fixture } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    stub.detailError = undefined;
    await component.retry();
    await settle(fixture);

    expect(text(fixture)).toContain('NCLEX Readiness');
  });

  it('renders the Stitch hero, backend-backed fact tiles, instructions summary and bottom actions', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelectorAll('.np-exam-detail-hero h1')).toHaveLength(1);
    expect(root.querySelectorAll('.np-exam-detail-facts > div')).toHaveLength(4);
    expect(root.querySelector('.np-exam-detail-summary h2')?.textContent).toContain('Exam instructions summary');
    expect(root.querySelector('.np-exam-detail-summary')?.textContent).toContain('Answer every question carefully.');
    expect(root.querySelector('.np-exam-detail-actions a[data-testid="exam-instructions-link"]')).not.toBeNull();
    expect(text(fixture)).not.toContain('Testing sessions operate under continuous timed progression');
  });

  it('uses a localized factual summary fallback rather than invented exam instructions', async () => {
    const stub = new ExamsApiStub();
    stub.detail = { ...STARTABLE, instructions: null, description: null };
    const { fixture } = await setup(stub);
    expect((fixture.nativeElement as HTMLElement).querySelector('.np-exam-detail-summary')?.textContent)
      .toContain('No instructions were supplied for this exam.');
    expect(text(fixture)).not.toContain('Are you ready?');
  });
});
