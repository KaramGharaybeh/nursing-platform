import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import { CheckoutScreen } from './checkout';

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

function createdOrder() {
  return {
    id: 'order-1',
    status: 'PendingPayment',
    currency: 'USD',
    totalAmountMinor: '4999',
    createdAt: '2026-02-01T00:00:00Z',
    items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
  };
}

class ConfirmationApi {
  getProduct() {
    return of(activeProduct());
  }

  createOrder() {
    return of(createdOrder());
  }
}

class MissingContextApi {
  getProduct() {
    return throwError(() => ({ status: 500 }));
  }

  createOrder() {
    return throwError(() => ({ status: 500 }));
  }
}

class UnavailableApi {
  getProduct() {
    return throwError(() => ({ status: 404 }));
  }

  createOrder() {
    return throwError(() => ({ status: 500 }));
  }
}

class ConflictApi {
  getProduct() {
    return of(activeProduct());
  }

  createOrder() {
    return throwError(() => ({ status: 409 }));
  }
}

function providers(api: unknown, query: Record<string, string> = { productId: 'product-1' }) {
  return [
    provideRouter([]),
    { provide: CommercePaymentsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { queryParamMap: convertToParamMap(query) } },
    },
    Router,
  ];
}

const meta: Meta<CheckoutScreen> = {
  component: CheckoutScreen,
  title: 'Features/Commerce/Checkout',
};
export default meta;
type Story = StoryObj<CheckoutScreen>;

export const Confirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ConfirmationApi()) })],
};

export const MissingContext: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingContextApi(), {}) })],
};

export const Unavailable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnavailableApi()) })],
};

export const Conflict: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ConflictApi()) })],
};
