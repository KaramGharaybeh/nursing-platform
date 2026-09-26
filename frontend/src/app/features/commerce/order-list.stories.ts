import { provideRouter, withDisabledInitialNavigation } from '@angular/router';
import { applicationConfig, type Meta, type StoryObj } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { CommercePaymentsApi } from '../../core/api/commerce-payments-api';
import type { CommerceOrderPage } from '../../core/api/commerce-payments-api';
import { OrderListScreen } from './order-list';

const page: CommerceOrderPage = {
  items: [{
    id: '52bc1507-7c14-43b4-b23c-a23cc1a8930d',
    status: 'PendingPayment', currency: 'USD', totalAmountMinor: '4999',
    createdAt: '2026-02-01T00:00:00Z',
    items: [{ title: 'NCLEX Mock Exam', currency: 'USD', lineTotalAmountMinor: '4999' }],
  }],
  page: 1, pageSize: 20, totalCount: 1, totalPages: 1,
};

const meta: Meta<OrderListScreen> = {
  component: OrderListScreen,
  title: 'Features/Commerce/OrderList',
};
export default meta;
type Story = StoryObj<OrderListScreen>;

function providers(result: CommerceOrderPage | 'error') {
  return [
    provideRouter([], withDisabledInitialNavigation()),
    { provide: CommercePaymentsApi, useValue: {
      listOrders: () => result === 'error' ? throwError(() => ({ status: 500 })) : of(result),
    } },
  ];
}

export const Populated: Story = {
  decorators: [applicationConfig({ providers: providers(page) })],
};
export const Empty: Story = {
  decorators: [applicationConfig({ providers: providers({ ...page, items: [], totalCount: 0, totalPages: 0 }) })],
};
export const LoadError: Story = {
  decorators: [applicationConfig({ providers: providers('error') })],
};
