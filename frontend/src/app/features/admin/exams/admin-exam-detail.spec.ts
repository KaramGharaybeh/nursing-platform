import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import type { AdminExamDto } from '../../../core/api/generated/models/admin-exam-dto';
import { AdminExamDetail } from './admin-exam-detail';

const exam: AdminExamDto = {
  id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
  categoryName: 'NCLEX', title: 'Mock exam', slug: 'mock-exam', description: 'Sample', instructions: null,
  durationMinutes: 90, passingScorePercentage: 70, status: 'Draft', isFree: true, publishedAt: null,
};

class ExamStub {
  current = exam;
  error: unknown;
  actionError: unknown;
  pendingArchive: Subject<AdminExamDto> | undefined;
  calls: string[] = [];
  get(id: string) { this.calls.push(`get:${id}`); return this.error ? throwError(() => this.error) : of(this.current); }
  update(id: string, body: unknown) { this.calls.push(`update:${id}:${JSON.stringify(body)}`); return this.actionError ? throwError(() => this.actionError) : of(this.current); }
  archive(id: string) { this.calls.push(`archive:${id}`); return this.pendingArchive?.asObservable() ?? (this.actionError ? throwError(() => this.actionError) : of({ ...this.current, status: 'Archived' })); }
  delete(id: string) { this.calls.push(`delete:${id}`); return this.actionError ? throwError(() => this.actionError) : of(undefined); }
}

async function setup(api = new ExamStub(), permissions = ['Exams.View', 'Exams.Edit', 'Exams.Delete']) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [AdminExamDetail], providers: [
    provideRouter([]), { provide: AdminExamsApi, useValue: api },
    { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } } },
    { provide: CountriesApi, useValue: { list: () => of([{ id: 'country-1', name: 'United States', code: 'US' }]) } },
    { provide: AdminExamCategoriesApi, useValue: { list: () => of({ items: [{ id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX', slug: 'nclex', displayOrder: 1, isActive: true }], page: 1, pageSize: 100, totalCount: 1, totalPages: 1 }) } },
    { provide: CurrentUserStore, useValue: { status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions }) } },
  ] }).compileComponents();
  const fixture = TestBed.createComponent(AdminExamDetail);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}
async function settle(f: ComponentFixture<AdminExamDetail>) {
  await f.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  f.detectChanges();
}
function view(f: ComponentFixture<AdminExamDetail>): HTMLElement { return f.nativeElement as HTMLElement; }
function click(f: ComponentFixture<AdminExamDetail>, id: string) {
  const button = view(f).querySelector<HTMLButtonElement>(`[data-testid="${id}"]`);
  if (!button) throw new Error(`Missing ${id}`);
  button.click();
}

describe('ADM-007 Admin exam detail', () => {
  it('renders safe facts and canonical Back to admin exams without protected content', async () => {
    const { fixture, api } = await setup();
    expect(api.calls).toContain('get:exam-1');
    expect(view(fixture).querySelector('h1')?.textContent).toBe('Mock exam');
    expect(view(fixture).textContent).toContain('90 minutes');
    expect(view(fixture).textContent).toContain('70%');
    expect(view(fixture).textContent).toContain('Draft');
    expect(view(fixture).textContent).not.toContain('exam-1');
    expect(view(fixture).textContent).not.toMatch(/correct answer|question text/i);
    expect(view(fixture).querySelector<HTMLAnchorElement>('[data-testid="back-to-exams"]')?.getAttribute('href')).toBe('/admin/exams');
    expect(view(fixture).querySelector('[data-testid="versions-link"]')).toBeNull();
  });

  it('hides mutations without exact backend permissions', async () => {
    const { fixture } = await setup(new ExamStub(), ['Exams.View']);
    for (const id of ['edit-exam', 'archive-exam', 'delete-exam']) {
      expect(view(fixture).querySelector(`[data-testid="${id}"]`)).toBeNull();
    }
  });

  it('requires explicit archive confirmation, prevents duplicate activation and reloads backend truth', async () => {
    const api = new ExamStub();
    api.pendingArchive = new Subject<AdminExamDto>();
    const { fixture } = await setup(api);
    click(fixture, 'archive-exam');
    await settle(fixture);
    expect(api.calls).toEqual(['get:exam-1']);
    expect(view(fixture).textContent).toContain('Archive exam?');
    click(fixture, 'confirm-action');
    click(fixture, 'confirm-action');
    expect(api.calls.filter((call) => call === 'archive:exam-1')).toHaveLength(1);
    api.current = { ...exam, status: 'Archived' };
    api.pendingArchive.next(api.current);
    api.pendingArchive.complete();
    await settle(fixture);
    expect(api.calls.filter((call) => call === 'get:exam-1')).toHaveLength(2);
    expect(view(fixture).textContent).toContain('Archived');
  });

  it('returns focus to the archive trigger when confirmation is dismissed', async () => {
    const { fixture, api } = await setup();
    const trigger = view(fixture).querySelector<HTMLButtonElement>('[data-testid="archive-exam"]');
    if (!trigger) throw new Error('Missing archive trigger');
    trigger.focus();
    trigger.click();
    await settle(fixture);
    expect(view(fixture).ownerDocument.activeElement?.textContent?.trim()).toBe('Archive exam?');
    const keep = [...view(fixture).querySelectorAll<HTMLButtonElement>('button')]
      .find((button) => button.textContent?.trim() === 'Keep exam');
    keep?.click();
    await settle(fixture);
    expect(view(fixture).ownerDocument.activeElement?.getAttribute('data-testid')).toBe('archive-exam');
    expect(api.calls).toEqual(['get:exam-1']);
  });

  it('only offers delete for a draft and confirms before sending', async () => {
    const api = new ExamStub();
    api.current = { ...exam, status: 'Published' };
    const { fixture } = await setup(api);
    expect(view(fixture).querySelector('[data-testid="delete-exam"]')).toBeNull();
    api.current = exam;
    const retry = fixture.componentInstance as unknown as { retry(): Promise<void> };
    void retry.retry();
    await settle(fixture);
    click(fixture, 'delete-exam');
    await settle(fixture);
    expect(api.calls).not.toContain('delete:exam-1');
    click(fixture, 'confirm-action');
    await settle(fixture);
    expect(api.calls).toContain('delete:exam-1');
  });

  it('uses privacy-safe unavailable/error states', async () => {
    const api = new ExamStub();
    api.error = { status: 404, error: { detail: 'Foreign exam id' } };
    const { fixture } = await setup(api);
    expect(view(fixture).textContent).toContain("This exam isn't available.");
    expect(view(fixture).textContent).not.toContain('Foreign exam id');
    expect(view(fixture).querySelector('[data-testid="edit-exam"]')).toBeNull();
  });

  it('maps update validation to the form without revealing server internals', async () => {
    const api = new ExamStub();
    api.actionError = { status: 400, error: { detail: 'private database path', errors: { 'Request.Title': ['sensitive constraint'] } } };
    const { fixture } = await setup(api);
    click(fixture, 'edit-exam');
    await settle(fixture);
    const detail = fixture.componentInstance as unknown as { save(body: object): Promise<void> };
    void detail.save({ countryId: 'country-1', title: 'Mock exam', slug: 'mock-exam', durationMinutes: 90, passingScorePercentage: 70, isFree: true });
    await settle(fixture);
    expect(view(fixture).textContent).toContain('Check the highlighted fields');
    expect(view(fixture).querySelector('#exam-title-error')?.textContent).toContain('Review title.');
    expect(view(fixture).textContent).not.toMatch(/private database path|sensitive constraint/);
  });

  it('mounts canonical guarded detail route', () => {
    const route = routes.find((entry) => entry.path === 'admin/exams/:examId');
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'ADMIN_EXAM_DETAIL' });
  });
});
