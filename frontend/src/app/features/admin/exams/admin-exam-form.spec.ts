import { TestBed } from '@angular/core/testing';
import type { CreateAdminExamRequest } from '../../../core/api/generated/models/create-admin-exam-request';
import { AdminExamForm } from './admin-exam-form';

describe('AdminExamForm (T-FE-107)', () => {
  async function setup(initial?: object) {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({ imports: [AdminExamForm] }).compileComponents();
    const fixture = TestBed.createComponent(AdminExamForm);
    fixture.componentRef.setInput('countries', [{ id: 'country-1', code: 'US', name: 'United States' }]);
    fixture.componentRef.setInput('categories', [
      { id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX', slug: 'nclex', displayOrder: 1, isActive: true },
    ]);
    if (initial) fixture.componentRef.setInput('exam', initial);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  it('rejects blank title and invalid scoring/duration without emitting a backend request', async () => {
    const fixture = await setup();
    const values: CreateAdminExamRequest[] = [];
    fixture.componentInstance.submitted.subscribe((request) => values.push(request));
    (fixture.componentInstance as unknown as { submit(): void }).submit();
    fixture.detectChanges();
    expect(values).toEqual([]);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Check the highlighted fields');
  });

  it('emits only supported fields with country-matching optional category and no raw ids on screen', async () => {
    const fixture = await setup();
    const values: CreateAdminExamRequest[] = [];
    fixture.componentInstance.submitted.subscribe((request) => values.push(request));
    const form = fixture.componentInstance as unknown as {
      updateCountry(value: string): void; updateCategory(value: string): void;
      updateTitle(value: string): void; updateSlug(value: string): void;
      updateDuration(value: string): void; updateScore(value: string): void;
      submit(): void;
    };
    form.updateCountry('country-1'); form.updateCategory('category-1');
    form.updateTitle('Mock exam'); form.updateSlug('mock-exam');
    form.updateDuration('90'); form.updateScore('70'); form.submit();
    expect(values).toEqual([{ countryId: 'country-1', examCategoryId: 'category-1', title: 'Mock exam',
      slug: 'mock-exam', description: null, instructions: null, durationMinutes: 90,
      passingScorePercentage: 70, isFree: true }]);
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('category-1');
  });

  it('prefills update values from backend detail and exposes no unsupported fields', async () => {
    const fixture = await setup({
      id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
      categoryName: 'NCLEX', title: 'Nursing exam', slug: 'nursing-exam', durationMinutes: 90,
      passingScorePercentage: 70, isFree: false, status: 'Published',
    });
    const form = fixture.componentInstance as unknown as { form: { controls: { title: { value: string }; isFree: { value: boolean } } } };
    expect(form.form.controls.title.value).toBe('Nursing exam');
    expect(form.form.controls.isFree.value).toBe(false);
    expect((fixture.nativeElement as HTMLElement).textContent).not.toMatch(/Correct answer|Publish now|Score calculation/i);
  });

  it('allows clearing an optional category while keeping the selected country', async () => {
    const fixture = await setup({
      id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
      categoryName: 'NCLEX', title: 'Nursing exam', slug: 'nursing-exam', durationMinutes: 90,
      passingScorePercentage: 70, isFree: true, status: 'Draft',
    });
    const values: CreateAdminExamRequest[] = [];
    fixture.componentInstance.submitted.subscribe((value) => values.push(value));
    const form = fixture.componentInstance as unknown as { categoryOptions: readonly { value: string }[]; updateCategory(value: string): void; submit(): void };
    expect(form.categoryOptions.some((option) => option.value === '')).toBe(true);
    form.updateCategory('');
    form.submit();
    expect(values[0]?.examCategoryId).toBeNull();
    expect(values[0]?.countryId).toBe('country-1');
  });
});
