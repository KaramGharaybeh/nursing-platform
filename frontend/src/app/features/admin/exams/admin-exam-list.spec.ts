import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import type { AdminExamDto } from '../../../core/api/generated/models/admin-exam-dto';
import { AdminExamList } from './admin-exam-list';

export const EXAM: AdminExamDto = {
  id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
  categoryName: 'NCLEX', title: 'Mock exam', slug: 'mock-exam', description: 'Review exam',
  instructions: null, durationMinutes: 90, passingScorePercentage: 70, status: 'Draft',
  isFree: true, publishedAt: null,
};

class ExamsStub {
  items: AdminExamDto[] = [EXAM];
  error: unknown;
  createError: unknown;
  calls: { page?: number; countryId?: string; categoryId?: string; status?: number; isFree?: boolean }[] = [];
  created: unknown[] = [];
  list(params: { page?: number; countryId?: string; categoryId?: string; status?: number; isFree?: boolean }) {
    this.calls.push(params);
    return this.error ? throwError(() => this.error) : of({ items: this.items, page: params.page ?? 1, pageSize: 20, totalCount: this.items.length ? 21 : 0, totalPages: this.items.length ? 2 : 0 });
  }
  create(body: unknown) { this.created.push(body); return this.createError ? throwError(() => this.createError) : of(EXAM); }
}

class LookupStub {
  fails = false;
  calls = 0;
  list() {
    this.calls++;
    return this.fails ? throwError(() => ({ status: 503 }))
      : of([{ id: 'country-1', name: 'United States', code: 'US' }]);
  }
}

async function setup(api = new ExamsStub(), permissions = ['Exams.View', 'Exams.Create'], lookup = new LookupStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [AdminExamList],
    providers: [
      provideRouter([]), { provide: AdminExamsApi, useValue: api },
      { provide: CountriesApi, useValue: lookup },
      { provide: AdminExamCategoriesApi, useValue: { list: () => of({ items: [{ id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX', slug: 'nclex', displayOrder: 1, isActive: true }], page: 1, pageSize: 100, totalCount: 1, totalPages: 1 }) } },
      { provide: CurrentUserStore, useValue: { status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions }) } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(AdminExamList);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api, lookup };
}

async function settle(fixture: ComponentFixture<AdminExamList>) {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

function root(fixture: ComponentFixture<AdminExamList>): HTMLElement { return fixture.nativeElement as HTMLElement; }

describe('ADM-006 Admin exam list', () => {
  it('shows safe backend facts with canonical detail link and exact page request', async () => {
    const { fixture, api } = await setup();
    expect(root(fixture).querySelector('h1')?.textContent).toBe('Admin exams');
    expect(root(fixture).textContent).toContain('Mock exam');
    expect(root(fixture).textContent).toContain('United States');
    expect(root(fixture).textContent).toContain('NCLEX');
    expect(root(fixture).textContent).toContain('Draft');
    expect(root(fixture).textContent).not.toContain('exam-1');
    expect(root(fixture).querySelector<HTMLAnchorElement>('[data-testid="exam-detail-link"]')?.getAttribute('href')).toBe('/admin/exams/exam-1');
    expect(api.calls).toEqual([{ page: 1 }]);
    root(fixture).querySelector<HTMLButtonElement>('[data-testid="pagination-next"]')?.click();
    await settle(fixture);
    expect(api.calls.at(-1)).toEqual({ page: 2 });
  });

  it('filters only through supported country/status backend params and resets page', async () => {
    const { fixture, api } = await setup();
    const select = root(fixture).querySelector<HTMLSelectElement>('#exam-status-filter');
    if (!select) throw new Error('Missing status filter');
    select.value = '0';
    select.dispatchEvent(new Event('change'));
    await settle(fixture);
    expect(api.calls.at(-1)).toEqual({ page: 1, status: 0 });
  });

  it('hides create from a viewer but retains the list', async () => {
    const { fixture } = await setup(new ExamsStub(), ['Exams.View']);
    expect(root(fixture).querySelector('[data-testid="create-exam"]')).toBeNull();
    expect(root(fixture).textContent).toContain('Mock exam');
  });

  it('presents empty and safe error states without leaking server text', async () => {
    const api = new ExamsStub();
    api.items = [];
    const { fixture } = await setup(api);
    expect(root(fixture).textContent).toContain('No exams yet');
    api.error = { status: 500, error: { detail: 'Secret DB path' } };
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };
    void component.retry();
    await settle(fixture);
    expect(root(fixture).textContent).not.toContain('Secret DB path');
    expect(root(fixture).textContent).toContain("We couldn't load admin exams. Try again.");
  });

  it('retries lookup initialization when it fails before the exam request', async () => {
    const lookup = new LookupStub();
    lookup.fails = true;
    const { fixture, api } = await setup(new ExamsStub(), ['Exams.View'], lookup);
    expect(api.calls).toEqual([]);
    expect(root(fixture).textContent).toContain("We couldn't load admin exams. Try again.");
    lookup.fails = false;
    root(fixture).querySelector<HTMLButtonElement>('.np-loading-error-retry-retry')?.click();
    await settle(fixture);
    expect(lookup.calls).toBe(2);
    expect(api.calls).toEqual([{ page: 1 }]);
  });

  it('maps create validation to the form without exposing backend messages', async () => {
    const api = new ExamsStub();
    api.createError = { status: 400, error: { detail: 'secret server detail', errors: {
      'Request.Title': ['internal constraint name'],
    } } };
    const { fixture } = await setup(api);
    root(fixture).querySelector<HTMLButtonElement>('[data-testid="create-exam"]')?.click();
    await settle(fixture);
    const form = root(fixture).querySelector('np-admin-exam-form');
    if (!form) throw new Error('Exam form missing');
    const submit = fixture.componentInstance as unknown as { create(body: object): Promise<void> };
    void submit.create({ countryId: 'country-1', title: 'Mock exam', slug: 'mock-exam', durationMinutes: 90, passingScorePercentage: 70, isFree: true });
    await settle(fixture);
    expect(root(fixture).textContent).toContain('Check the highlighted fields');
    expect(root(fixture).querySelector('#exam-title-error')?.textContent).toContain('Review title.');
    expect(root(fixture).textContent).not.toMatch(/secret server detail|internal constraint name/);
  });

  it('moves focus to the new form heading and returns it to Add exam on cancel', async () => {
    const { fixture } = await setup();
    const trigger = root(fixture).querySelector<HTMLButtonElement>('[data-testid="create-exam"]');
    if (!trigger) throw new Error('Missing Add exam');
    trigger.focus();
    trigger.click();
    await settle(fixture);
    expect(root(fixture).ownerDocument.activeElement?.tagName).toBe('H2');
    expect(root(fixture).ownerDocument.activeElement?.textContent?.trim()).toBe('Add exam');
    const cancel = [...root(fixture).querySelectorAll<HTMLButtonElement>('button')]
      .find((button) => button.textContent?.trim() === 'Cancel');
    cancel?.click();
    await settle(fixture);
    expect(root(fixture).ownerDocument.activeElement).toBe(trigger);
  });

  it('mounts exact canonical route with all three guards', () => {
    const route = routes.find((entry) => entry.path === 'admin/exams');
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'ADMIN_EXAMS' });
  });
});
