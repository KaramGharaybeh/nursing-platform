import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { ContactRequestsApi } from '../../../core/api/contact-requests-api';
import { NurseContactRequests } from './nurse-contact-requests';

const PENDING = {
  id: 'req-1',
  organizationName: 'City General Hospital',
  jobTitle: 'Emergency Nurse',
  department: 'Emergency',
  status: 'Pending',
  createdAt: '2026-09-10T10:00:00Z',
  updatedAt: '2026-09-10T10:00:00Z',
};

const MIXED = [
  PENDING,
  {
    id: 'req-2',
    organizationName: 'Riverside Clinic',
    status: 'Approved',
    createdAt: '2026-09-08T10:00:00Z',
    updatedAt: '2026-09-09T10:00:00Z',
    respondedAt: '2026-09-09T10:00:00Z',
  },
  {
    id: 'req-3',
    organizationName: 'North Regional Medical Center for Advanced Care',
    jobTitle: 'Senior Intensive Care Registered Nurse',
    department: 'Cardiothoracic Intensive Care',
    status: 'Rejected',
    createdAt: '2026-09-05T10:00:00Z',
    updatedAt: '2026-09-06T10:00:00Z',
    respondedAt: '2026-09-06T10:00:00Z',
  },
  {
    id: 'req-4',
    organizationName: 'Harbor Institute',
    status: 'Cancelled',
    createdAt: '2026-09-01T10:00:00Z',
    updatedAt: '2026-09-02T10:00:00Z',
  },
];

function pageOf(items: typeof MIXED, totalCount: number) {
  return of({
    items,
    page: 1,
    pageSize: 20,
    totalCount,
    totalPages: Math.max(1, Math.ceil(totalCount / 20)),
  });
}

class PopulatedApi {
  listReceived() {
    return pageOf([PENDING], 1);
  }
  approve() {
    return of({ ...PENDING, status: 'Approved' });
  }
  reject() {
    return of({ ...PENDING, status: 'Rejected' });
  }
}

class MixedApi extends PopulatedApi {
  override listReceived() {
    return pageOf(MIXED, MIXED.length);
  }
}

class EmptyApi extends PopulatedApi {
  override listReceived() {
    return pageOf([], 0);
  }
}

const STATUS_BY_INDEX: readonly (string | undefined)[] = [undefined, 'Pending', 'Approved', 'Rejected', 'Cancelled'];

class FilteringApi extends PopulatedApi {
  override listReceived(query: { status?: number }) {
    const wanted = query.status === undefined ? undefined : STATUS_BY_INDEX[query.status + 1];
    const items = wanted === undefined ? [PENDING] : [];
    return pageOf(items, items.length);
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: ContactRequestsApi, useValue: api }];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<NurseContactRequests> = {
  component: NurseContactRequests,
  title: 'Features/Nurse/ContactRequests',
};
export default meta;
type Story = StoryObj<NurseContactRequests>;

export const Pending: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const MixedStatuses: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MixedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const FilteredNoResults: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new FilteringApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const select = canvasElement.querySelector('mat-select');
    if (select === null) {
      throw new Error('Contact requests story could not find the status select.');
    }
    (select as HTMLElement).click();
    await settle();
    const option = Array.from(canvasElement.ownerDocument.querySelectorAll('[role="option"]')).find((entry) =>
      (entry.textContent ?? '').includes('Approved'),
    ) as HTMLElement | undefined;
    if (option === undefined) {
      throw new Error('Contact requests story could not find the Approved option.');
    }
    option.click();
    await settle();
  },
};

export const RejectConfirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const remove = canvasElement.querySelector<HTMLElement>('[data-testid="request-reject"]');
    if (remove === null) {
      throw new Error('Contact requests story could not find the Reject action.');
    }
    remove.click();
    await settle();
  },
};
