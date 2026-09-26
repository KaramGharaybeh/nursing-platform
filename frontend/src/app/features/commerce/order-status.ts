import type { CommerceOrderStatus } from '../../core/api/commerce-payments-api';

export function orderStatusLabel(status: CommerceOrderStatus): string {
  switch (status) {
    case 'PendingPayment': return 'Pending payment';
    case 'Paid': return 'Paid';
    case 'Failed': return 'Failed';
    case 'Cancelled': return 'Cancelled';
    case 'Expired': return 'Expired';
  }
}
