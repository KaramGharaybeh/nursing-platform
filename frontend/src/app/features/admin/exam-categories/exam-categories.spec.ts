import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import type { AdminExamCategoryDto } from '../../../core/api/generated/models/admin-exam-category-dto';
import { ExamCategoriesScreen } from './exam-categories';

const category: AdminExamCategoryDto = {
  id: 'category-1', countryId: 'country-1', countryName: 'United States',
  name: 'NCLEX', slug: 'nclex', description: 'Nursing exam', displayOrder: 2, isActive: true,
};

class CategoryStub {
  items = [category];
  error: unknown;
  actionError: unknown;
  calls: { page: number; pageSize: number; countryId?: string; isActive?: boolean }[] = [];
  actions: string[] = [];
  updateBodies: unknown[] = [];
  list(params: { page: number; pageSize: number; countryId?: string; isActive?: boolean }) {
    this.calls.push(params);
    return this.error ? throwError(() => this.error) : of({ items: this.items, page: params.page, pageSize: 20, totalCount: 21, totalPages: 2 });
  }
  get(id: string) { this.actions.push(`get:${id}`); return of(category); }
  create(body: { name: string }) { this.actions.push(`create:${body.name}`); return this.result(); }
  update(id: string, body: { name: string }) { this.actions.push(`update:${id}:${body.name}`); this.updateBodies.push(body); return this.result(); }
  archive(id: string) { this.actions.push(`archive:${id}`); return this.result(); }
  restore(id: string) { this.actions.push(`restore:${id}`); return this.result(); }
  delete(id: string) { this.actions.push(`delete:${id}`); return this.actionError ? throwError(() => this.actionError) : of(undefined); }
  private result() { return this.actionError ? throwError(() => this.actionError) : of(category); }
}

async function setup(permissions = ['Exams.View', 'Exams.Create', 'Exams.Edit', 'Exams.Delete'], api = new CategoryStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [ExamCategoriesScreen],
    providers: [
      provideRouter([]),
      { provide: AdminExamCategoriesApi, useValue: api },
      { provide: CountriesApi, useValue: { list: () => of([{ id: 'country-1', name: 'United States', code: 'US' }]) } },
      { provide: CurrentUserStore, useValue: { status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions }) } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(ExamCategoriesScreen);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}
async function settle(fixture: ComponentFixture<ExamCategoriesScreen>) {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}
function root(f: ComponentFixture<ExamCategoriesScreen>): HTMLElement { return f.nativeElement as HTMLElement; }
function click(f: ComponentFixture<ExamCategoriesScreen>, id: string) {
  const control = root(f).querySelector<HTMLButtonElement>(`[data-testid="${id}"]`);
  if (!control) throw new Error(`Missing action ${id}`);
  control.click();
}

describe('ADM-005 exam categories (T-FE-106)', () => {
  it('renders only category facts and backend paging; excludes generic reference-data claims', async () => {
    const { fixture, api } = await setup();
    expect(root(fixture).querySelector('h1')?.textContent).toBe('Exam categories');
    expect(root(fixture).textContent).toContain('NCLEX');
    expect(root(fixture).textContent).toContain('United States');
    expect(root(fixture).textContent).toContain('nclex');
    expect(root(fixture).textContent).not.toContain('category-1');
    expect(root(fixture).textContent).not.toMatch(/manage countries|manage languages/i);
    expect(api.calls).toEqual([{ page: 1, pageSize: 20 }]);
    click(fixture, 'pagination-next');
    await settle(fixture);
    expect(api.calls[1]).toEqual({ page: 2, pageSize: 20 });
  });

  it('only offers actions granted by exact backend permissions', async () => {
    const { fixture } = await setup(['Exams.View']);
    for (const action of ['add-category', 'edit-category', 'archive-category', 'delete-category']) {
      expect(root(fixture).querySelector(`[data-testid="${action}"]`)).toBeNull();
    }
    expect(root(fixture).textContent).toContain('NCLEX');
  });

  it('creates a category with explicit country and supported fields, then reloads server truth', async () => {
    const { fixture, api } = await setup();
    click(fixture, 'add-category');
    await settle(fixture);
    const component = fixture.componentInstance as unknown as {
      updateCountry(id: string): void;
      updateName(value: string): void;
      updateSlug(value: string): void;
      updateDisplayOrder(value: string): void;
    };
    component.updateCountry('country-1');
    component.updateName('NCLEX');
    component.updateSlug('nclex');
    component.updateDisplayOrder('2');
    click(fixture, 'save-category');
    await settle(fixture);
    expect(api.actions).toContain('create:NCLEX');
    expect(api.calls.length).toBe(2);
    expect(root(fixture).textContent).toContain('Exam category created.');
  });

  it('moves keyboard focus to the newly opened form heading', async () => {
    const { fixture } = await setup();
    click(fixture, 'add-category');
    await settle(fixture);
    expect((root(fixture).ownerDocument.activeElement as HTMLElement).textContent?.trim()).toBe('Add exam category');
  });

  it('keeps country immutable when editing and sends only editable fields', async () => {
    const { fixture, api } = await setup();
    click(fixture, 'edit-category');
    await settle(fixture);
    expect(api.actions).toContain('get:category-1');
    expect(root(fixture).querySelector('#category-country')?.getAttribute('aria-disabled')).toBe('true');
    click(fixture, 'save-category');
    await settle(fixture);
    expect(api.actions).toContain('update:category-1:NCLEX');
    expect(api.updateBodies[0]).not.toHaveProperty('countryId');
  });

  it('uses only backed country/status filters and resets to page one', async () => {
    const { fixture, api } = await setup();
    click(fixture, 'pagination-next');
    await settle(fixture);
    const select = root(fixture).querySelector<HTMLSelectElement>('#category-active-filter');
    if (!select) throw new Error('Status filter missing');
    select.value = 'archived';
    select.dispatchEvent(new Event('change'));
    await settle(fixture);
    expect(api.calls.at(-1)).toEqual({ page: 1, pageSize: 20, isActive: false });
  });

  it('does not send whitespace identity or non-integer display order on create', async () => {
    const { fixture, api } = await setup();
    click(fixture, 'add-category');
    await settle(fixture);
    const component = fixture.componentInstance as unknown as {
      updateCountry(id: string): void;
      updateName(value: string): void;
      updateSlug(value: string): void;
      updateDisplayOrder(value: string): void;
    };
    component.updateCountry('country-1');
    component.updateName('   ');
    component.updateSlug('nclex');
    component.updateDisplayOrder('1.5');
    click(fixture, 'save-category');
    await settle(fixture);
    expect(api.actions).toEqual([]);
    expect(root(fixture).textContent).toContain('Complete the required fields');
  });

  it('maps backend validation to named controls without exposing raw backend messages', async () => {
    const api = new CategoryStub();
    api.actionError = { status: 400, error: {
      errors: { 'Request.Name': ['internal-db-key'], 'Request.Slug': ['provider secret'] },
      detail: 'private server detail',
    } };
    const { fixture } = await setup(['Exams.View', 'Exams.Create'], api);
    click(fixture, 'add-category');
    await settle(fixture);
    const form = fixture.componentInstance as unknown as {
      updateCountry(id: string): void; updateName(value: string): void; updateSlug(value: string): void;
    };
    form.updateCountry('country-1'); form.updateName('NCLEX'); form.updateSlug('nclex');
    click(fixture, 'save-category');
    await settle(fixture);
    expect(root(fixture).textContent).toContain('Check the highlighted fields');
    expect(root(fixture).querySelector('#category-name-error')?.textContent).toContain('Review name.');
    expect(root(fixture).querySelector('#category-name')?.getAttribute('aria-describedby')).toContain('category-name-error');
    expect(root(fixture).textContent).not.toMatch(/internal-db-key|provider secret|private server detail/);
  });

  it('confirms archive before mutation and reconciles the backend list', async () => {
    const { fixture, api } = await setup();
    click(fixture, 'archive-category');
    await settle(fixture);
    expect(api.actions).toEqual([]);
    expect(root(fixture).textContent).toContain('Archive exam category?');
    click(fixture, 'confirm-action');
    await settle(fixture);
    expect(api.actions).toEqual(['archive:category-1']);
    expect(api.calls.length).toBe(2);
  });

  it('returns focus to the archive trigger after choosing Keep category', async () => {
    const { fixture, api } = await setup();
    const trigger = root(fixture).querySelector<HTMLButtonElement>('[data-testid="archive-category"]');
    if (!trigger) throw new Error('Missing archive action');
    trigger.focus();
    trigger.click();
    await settle(fixture);
    expect(root(fixture).ownerDocument.activeElement?.textContent).toContain('Archive exam category?');
    const keep = [...root(fixture).querySelectorAll<HTMLButtonElement>('button')]
      .find((item) => item.textContent?.trim() === 'Keep category');
    keep?.click();
    await settle(fixture);
    expect(root(fixture).ownerDocument.activeElement).toBe(trigger);
    expect(api.actions).toEqual([]);
  });

  it('requires explicit delete confirmation and reports conflict without retry', async () => {
    const api = new CategoryStub();
    api.actionError = { status: 409, error: { detail: 'Internal dependency id' } };
    const { fixture } = await setup(['Exams.View', 'Exams.Delete'], api);
    click(fixture, 'delete-category');
    await settle(fixture);
    expect(api.actions).toEqual([]);
    click(fixture, 'confirm-action');
    await settle(fixture);
    expect(api.actions).toEqual(['delete:category-1']);
    expect(root(fixture).textContent).not.toContain('Internal dependency id');
    expect(root(fixture).textContent).toContain("This exam category couldn't be deleted.");
  });

  it('restores only an archived category with edit permission and reloads backend state', async () => {
    const api = new CategoryStub();
    api.items = [{ ...category, isActive: false }];
    const { fixture } = await setup(['Exams.View', 'Exams.Edit'], api);
    expect(root(fixture).querySelector('[data-testid="archive-category"]')).toBeNull();
    click(fixture, 'restore-category');
    await settle(fixture);
    expect(api.actions).toEqual(['restore:category-1']);
    expect(api.calls.length).toBe(2);
  });

  it('shows safe error and retries a failed list load', async () => {
    const api = new CategoryStub();
    api.error = { status: 500, error: { detail: 'Server internals' } };
    const { fixture } = await setup(['Exams.View'], api);
    expect(root(fixture).textContent).not.toContain('Server internals');
    api.error = undefined;
    root(fixture).querySelector<HTMLButtonElement>('.np-loading-error-retry-retry')?.click();
    await settle(fixture);
    expect(api.calls.length).toBe(2);
    expect(root(fixture).textContent).toContain('NCLEX');
  });

  it('mounts canonical route with three guards and exact Admin policy', () => {
    const route = routes.find((entry) => entry.path === 'admin/reference-data');
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'ADMIN_REFERENCE_DATA' });
  });
});
