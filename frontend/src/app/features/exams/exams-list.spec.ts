import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../app.routes';
import { ExamsApi } from '../../core/api/exams-api';
import type { CountryOption, ExamCatalogItem, ExamCatalogPage } from '../../core/api/exams-api';
import { ExamsList } from './exams-list';

const ITEM_1: ExamCatalogItem = {
  id: '11111111-1111-4111-8111-111111111111',
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
};

const ITEM_2: ExamCatalogItem = {
  id: '22222222-2222-4222-8222-222222222222',
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
};

function pageOf(items: ExamCatalogItem[], page = 1): ExamCatalogPage {
  return { items, page, pageSize: 20, totalCount: items.length, totalPages: 1 };
}

const COUNTRIES: CountryOption[] = [
  { id: 'country-1', name: 'Jordan' },
  { id: 'country-2', name: 'Egypt' },
];

class ExamsApiStub {
  catalog: ExamCatalogPage = pageOf([ITEM_1, ITEM_2]);
  catalogError: unknown = undefined;
  requested: { page: number; countryId?: string; categoryId?: string }[] = [];

  listExams(query: { page: number; pageSize: number; countryId?: string; categoryId?: string }) {
    this.requested.push({ page: query.page, countryId: query.countryId, categoryId: query.categoryId });
    if (this.catalogError !== undefined) {
      return throwError(() => this.catalogError);
    }
    return of(this.catalog);
  }

  getExam() {
    return throwError(() => ({ status: 404 }));
  }

  listCountries() {
    return of(COUNTRIES);
  }
}

async function setup(stub?: ExamsApiStub): Promise<{ fixture: ComponentFixture<ExamsList>; api: ExamsApiStub }> {
  const api = stub ?? new ExamsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamsList],
    providers: [provideRouter([]), { provide: ExamsApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamsList);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

async function settle(fixture: ComponentFixture<ExamsList>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function text(fixture: ComponentFixture<ExamsList>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<ExamsList>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('ExamsList (T-FE-067)', () => {
  it('loads the first page and renders approved card fields', async () => {
    const { fixture, api } = await setup();
    const content = text(fixture);

    expect(api.requested).toEqual([{ page: 1, countryId: undefined, categoryId: undefined }]);
    expect(content).toContain('NCLEX Readiness');
    expect(content).toContain('Jordan');
    expect(content).toContain('Licensure');
    expect(content).toContain('120 minutes');
    expect(content).toContain('75 questions');
    expect(content).toContain('Free');
    expect(content).not.toMatch(UUID_PATTERN);
    expect(content).not.toContain('Requires purchase');
  });

  it('hides the description and Free marker when not applicable', async () => {
    const stub = new ExamsApiStub();
    stub.catalog = pageOf([ITEM_2]);
    const { fixture } = await setup(stub);
    const content = text(fixture);

    expect(content).not.toContain('Are you ready?');
    expect(content).not.toMatch(/\bFree\b/);
  });

  it('sends the selected Country filter and resets to page one', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      onCountryChange(countryId: string): Promise<void>;
    };

    await component.onCountryChange('country-2');
    await settle(fixture);

    expect(api.requested.at(-1)).toEqual({ page: 1, countryId: 'country-2', categoryId: undefined });
  });

  it('sends the selected Category filter and resets to page one', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      onCategoryChange(categoryId: string): Promise<void>;
    };

    await component.onCategoryChange('cat-1');
    await settle(fixture);

    expect(api.requested.at(-1)).toEqual({ page: 1, countryId: undefined, categoryId: 'cat-1' });
  });

  it('offers country options from the lookup and category options from loaded items', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      countryOptions(): { value: string; label: string }[];
      categoryOptions(): { value: string; label: string }[];
    };

    expect(component.countryOptions()).toEqual([
      { value: '', label: 'All countries' },
      { value: 'country-1', label: 'Jordan' },
      { value: 'country-2', label: 'Egypt' },
    ]);
    expect(component.categoryOptions()).toEqual([
      { value: '', label: 'All categories' },
      { value: 'cat-1', label: 'Licensure' },
    ]);
  });

  it('passes page and pageSize through pagination', async () => {
    const stub = new ExamsApiStub();
    stub.catalog = { items: [ITEM_1], page: 2, pageSize: 20, totalCount: 21, totalPages: 2 };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { loadPage(page: number): Promise<void> };

    await component.loadPage(2);
    await settle(fixture);

    expect(api.requested.at(-1)).toEqual({ page: 2, countryId: undefined, categoryId: undefined });
  });

  it('shows a true empty state without filters', async () => {
    const stub = new ExamsApiStub();
    stub.catalog = pageOf([]);
    const { fixture } = await setup(stub);

    expect(text(fixture)).toContain('No exams available.');
  });

  it('shows a filtered no-results state when filters are active', async () => {
    const stub = new ExamsApiStub();
    stub.catalog = pageOf([]);
    const { fixture } = await setup(stub);
    const component = fixture.componentInstance as unknown as {
      onCountryChange(countryId: string): Promise<void>;
    };

    await component.onCountryChange('country-2');
    await settle(fixture);

    expect(text(fixture)).toContain('No exams match the selected filters.');
  });

  it('retries a backend error preserving filter and page context', async () => {
    const stub = new ExamsApiStub();
    stub.catalogError = { status: 500 };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    stub.catalogError = undefined;
    await component.retry();
    await settle(fixture);

    expect(api.requested.length).toBe(2);
    expect(text(fixture)).toContain('NCLEX Readiness');
  });

  it('links each card to its detail route', async () => {
    const { fixture } = await setup();
    const link = byTestId(fixture, 'exam-details-link') as HTMLAnchorElement | null;

    expect(link).not.toBeNull();
    expect(link?.getAttribute('href')).toBe('/exams/11111111-1111-4111-8111-111111111111');
  });
});

describe('Exams routes', () => {
  it('mounts /exams with the three-guard pattern and EXAMS_CATALOG routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_CATALOG' });
  });

  it('mounts /exams/:examId with the three-guard pattern and EXAMS_DETAIL routeId', () => {
    const route = routes.find((entry) => entry.path === 'exams/:examId');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'EXAMS_DETAIL' });
  });
});
