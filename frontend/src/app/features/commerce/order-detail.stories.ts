import { ActivatedRoute, convertToParamMap, provideRouter, withDisabledInitialNavigation } from '@angular/router';
import { applicationConfig, type Meta, type StoryObj } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrder } from '../../core/api/commerce-payments-api';
import { OrderDetailScreen } from './order-detail';

const pending: CommerceOrder = {
  id: '52bc1507-7c14-43b4-b23c-a23cc1a8930d',
  status: 'PendingPayment', currency: 'USD', totalAmountMinor: '4999',
  createdAt: '2026-02-01T00:00:00Z',
  items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
};

const meta: Meta<OrderDetailScreen> = {
  component: OrderDetailScreen,
  title: 'Features/Commerce/OrderDetail',
};
export default meta;
type Story = StoryObj<OrderDetailScreen>;

function providers(order: CommerceOrder | 'unavailable' | 'error') {
  return [
    provideRouter([], withDisabledInitialNavigation()),
    { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ orderId: pending.id }) } } },
    { provide: CommercePaymentsApi, useValue: {
      getOrder: () => order === 'unavailable' ? throwError(() => ({ status: 404 }))
        : order === 'error' ? throwError(() => ({ status: 500 })) : of(order),
      cancelOrder: () => of({ ...pending, status: 'Cancelled' }),
    } },
  ];
}

export const Pending: Story = {
  decorators: [applicationConfig({ providers: providers(pending) })],
};
export const Paid: Story = {
  decorators: [applicationConfig({ providers: providers({ ...pending, status: 'Paid', paidAt: '2026-02-02T00:00:00Z' }) })],
};
export const Unavailable: Story = {
  decorators: [applicationConfig({ providers: providers('unavailable') })],
};
export const LoadError: Story = {
  decorators: [applicationConfig({ providers: providers('error') })],
};
