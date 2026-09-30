import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import { ProductListScreen } from './product-list';

function populatedPage() {
  return {
    items: [
      {
        id: 'product-1',
        name: 'NCLEX Mock Exam',
        description: 'A full mock exam.',
        examTitle: 'NCLEX Readiness',
        currency: 'USD',
        unitAmountMinor: '4999',
        isActive: true,
      },
      {
        id: 'product-2',
        name: 'Second product',
        description: null,
        examTitle: null,
        currency: 'USD',
        unitAmountMinor: '1999',
        isActive: true,
      },
    ],
    page: 1,
    pageSize: 20,
    totalCount: 2,
    totalPages: 1,
  };
}

class PopulatedApi {
  listProducts() {
    return of(populatedPage());
  }
}

class EmptyApi {
  listProducts() {
    return of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  }
}

class LoadErrorApi {
  listProducts() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: unknown) {
  return [
    provideRouter([]),
    { provide: CommercePaymentsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({}) } },
    },
    Router,
  ];
}

const meta: Meta<ProductListScreen> = {
  component: ProductListScreen,
  title: 'Features/Commerce/ProductList',
};
export default meta;
type Story = StoryObj<ProductListScreen>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};
