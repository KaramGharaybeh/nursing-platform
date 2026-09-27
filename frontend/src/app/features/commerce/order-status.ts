import type { CommerceOrderStatus } from '../../core/api/commerce-payments-api';

export function orderStatusLabel(
  status: CommerceOrderStatus,
  labels: Record<CommerceOrderStatus, string>,
): string {
  return labels[status];
}
