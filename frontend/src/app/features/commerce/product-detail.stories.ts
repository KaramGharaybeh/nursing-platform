import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import { ProductDetailScreen } from './product-detail';

function activeProduct() {
  return {
    id: 'product-1',
    name: 'NCLEX Mock Exam',
    description: 'A full mock exam.',
    examTitle: 'NCLEX Readiness',
    currency: 'USD',
    unitAmountMinor: '4999',
    isActive: true,
  };
}

class ActiveApi {
  getProduct() {
    return of(activeProduct());
  }
}

class InactiveApi {
  getProduct() {
    return of({ ...activeProduct(), isActive: false });
  }
}

class NoDescriptionApi {
  getProduct() {
    return of({ ...activeProduct(), description: null, examTitle: null });
  }
}

class UnavailableApi {
  getProduct() {
    return throwError(() => ({ status: 404 }));
  }
}

class LoadErrorApi {
  getProduct() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: unknown) {
  return [
    provideRouter([]),
    { provide: CommercePaymentsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ productId: 'product-1' }) } },
    },
    Router,
  ];
}

const meta: Meta<ProductDetailScreen> = {
  component: ProductDetailScreen,
  title: 'Features/Commerce/ProductDetail',
};
export default meta;
type Story = StoryObj<ProductDetailScreen>;

export const Active: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ActiveApi()) })],
};

export const Inactive: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new InactiveApi()) })],
};

export const NoDescription: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NoDescriptionApi()) })],
};

export const Unavailable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnavailableApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};
