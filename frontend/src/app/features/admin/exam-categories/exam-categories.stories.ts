import { signal } from '@angular/core';
import { applicationConfig, type Meta, type StoryObj } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { AdminExamCategoriesApi } from '../../../core/api/admin-exam-categories-api';
import { CountriesApi } from '../../../core/api/countries-api';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { ExamCategoriesScreen } from './exam-categories';

const category = {
  id: 'category-1', countryId: 'country-1', countryName: 'United States', name: 'NCLEX',
  slug: 'nclex', description: 'Nursing exam', displayOrder: 2, isActive: true,
};

function providers(mode: 'populated' | 'empty' | 'error') {
  return [
    { provide: CountriesApi, useValue: { list: () => of([{ id: 'country-1', name: 'United States', code: 'US' }]) } },
    { provide: CurrentUserStore, useValue: {
      status: signal('ready'), currentUser: signal({ roles: ['Admin'], permissions: ['Exams.View', 'Exams.Create', 'Exams.Edit', 'Exams.Delete'] }),
    } },
    { provide: AdminExamCategoriesApi, useValue: {
      list: () => mode === 'error' ? throwError(() => ({ status: 500 })) : of({
        items: mode === 'empty' ? [] : [category], page: 1, pageSize: 20,
        totalCount: mode === 'empty' ? 0 : 1, totalPages: mode === 'empty' ? 0 : 1,
      }),
      get: () => of(category), create: () => of(category), update: () => of(category),
      archive: () => of({ ...category, isActive: false }), restore: () => of(category),
      delete: () => of(undefined),
    } },
  ];
}

const meta: Meta<ExamCategoriesScreen> = {
  title: 'Features/Admin/ExamCategories', component: ExamCategoriesScreen,
};
export default meta;
type Story = StoryObj<ExamCategoriesScreen>;

export const Populated: Story = { decorators: [applicationConfig({ providers: providers('populated') })] };
export const Empty: Story = { decorators: [applicationConfig({ providers: providers('empty') })] };
export const LoadError: Story = { decorators: [applicationConfig({ providers: providers('error') })] };
