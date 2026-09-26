import { signal } from '@angular/core';
import { provideRouter, withDisabledInitialNavigation } from '@angular/router';
import { applicationConfig, type Meta, type StoryObj } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { AdminExamsApi } from '../../../core/api/admin-exams-api';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { AdminExamList } from './admin-exam-list';

const exam = {
  id: 'exam-1', countryId: 'country-1', countryName: 'United States', examCategoryId: 'category-1',
  categoryName: 'NCLEX', title: 'Mock exam', slug: 'mock-exam', durationMinutes: 90,
  passingScorePercentage: 70, status: 'Draft', isFree: true,
};
const category = {
  id: 'category-1', countryId: 'country-1', countryName: 'United States',
  name: 'NCLEX', slug: 'nclex', displayOrder: 1, isActive: true,
};

function providers(mode: 'populated' | 'empty' | 'error') {
  return [
    provideRouter([], withDisabledInitialNavigation()),
    { provide: CurrentUserStore, useValue: { status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions: ['Exams.View', 'Exams.Create'] }) } },
    { provide: CountriesApi, useValue: { list: () => of([{ id: 'country-1', name: 'United States', code: 'US' }]) } },
    { provide: AdminExamCategoriesApi, useValue: { list: () => of({ items: [category], page: 1, pageSize: 100, totalCount: 1, totalPages: 1 }) } },
    { provide: AdminExamsApi, useValue: { list: () => mode === 'error' ? throwError(() => ({ status: 500 })) : of({
      items: mode === 'empty' ? [] : [exam], page: 1, pageSize: 20,
      totalCount: mode === 'empty' ? 0 : 1, totalPages: mode === 'empty' ? 0 : 1,
    }), create: () => of(exam) } },
  ];
}

const meta: Meta<AdminExamList> = { title: 'Features/Admin/ExamList', component: AdminExamList };
export default meta;
type Story = StoryObj<AdminExamList>;
export const Populated: Story = { decorators: [applicationConfig({ providers: providers('populated') })] };
export const Empty: Story = { decorators: [applicationConfig({ providers: providers('empty') })] };
export const LoadError: Story = { decorators: [applicationConfig({ providers: providers('error') })] };
