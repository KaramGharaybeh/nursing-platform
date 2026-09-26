import { signal } from '@angular/core';
import { ActivatedRoute, convertToParamMap, provideRouter, withDisabledInitialNavigation } from '@angular/router';
import { applicationConfig, type Meta, type StoryObj } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { AdminExamDetail } from './admin-exam-detail';

const exam = {
  id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
  categoryName: 'NCLEX', title: 'Mock exam', slug: 'mock-exam', description: 'An exam for review.',
  instructions: null, durationMinutes: 90, passingScorePercentage: 70, status: 'Draft',
  isFree: true, publishedAt: null,
};

function providers(mode: 'draft' | 'published' | 'unavailable' | 'error') {
  return [
    provideRouter([], withDisabledInitialNavigation()),
    { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ examId: 'exam-1' }) } } },
    { provide: CurrentUserStore, useValue: { status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions: ['Exams.View', 'Exams.Edit', 'Exams.Delete'] }) } },
    { provide: CountriesApi, useValue: { list: () => of([{ id: 'country-1', name: 'United States', code: 'US' }]) } },
    { provide: AdminExamCategoriesApi, useValue: { list: () => of({ items: [{ id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX', slug: 'nclex', displayOrder: 1, isActive: true }], page: 1, pageSize: 100, totalCount: 1, totalPages: 1 }) } },
    { provide: AdminExamsApi, useValue: { get: () => mode === 'unavailable' || mode === 'error'
      ? throwError(() => ({ status: mode === 'unavailable' ? 404 : 500 }))
      : of(mode === 'published' ? { ...exam, status: 'Published', publishedAt: '2026-02-02T00:00:00Z' } : exam),
      update: () => of(exam), archive: () => of({ ...exam, status: 'Archived' }), delete: () => of(undefined) } },
  ];
}

const meta: Meta<AdminExamDetail> = { title: 'Features/Admin/ExamDetail', component: AdminExamDetail };
export default meta;
type Story = StoryObj<AdminExamDetail>;
export const Draft: Story = { decorators: [applicationConfig({ providers: providers('draft') })] };
export const Published: Story = { decorators: [applicationConfig({ providers: providers('published') })] };
export const Unavailable: Story = { decorators: [applicationConfig({ providers: providers('unavailable') })] };
export const LoadError: Story = { decorators: [applicationConfig({ providers: providers('error') })] };
