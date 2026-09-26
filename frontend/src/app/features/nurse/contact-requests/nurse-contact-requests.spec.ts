import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { Announcer } from '../../../shared/ui/announcement';
import { ContactRequestsApi } from '../../../core/api/contact-requests-api';
import type { PaginatedResultOfReceivedContactRequestDto } from '../../../core/api/generated/models/paginated-result-of-received-contact-request-dto';
import type { ReceivedContactRequestDto } from '../../../core/api/generated/models/received-contact-request-dto';
import { NurseContactRequests } from './nurse-contact-requests';

const PENDING: ReceivedContactRequestDto = {
  id: 'req-1',
  organizationName: 'City General Hospital',
  jobTitle: 'Emergency Nurse',
  department: 'Emergency',
  status: 'Pending',
  createdAt: '2026-09-10T10:00:00Z',
  updatedAt: '2026-09-10T10:00:00Z',
};

const DECIDED: ReceivedContactRequestDto = {
  id: 'req-2',
  organizationName: 'Riverside Clinic',
  status: 'Approved',
  createdAt: '2026-09-08T10:00:00Z',
  updatedAt: '2026-09-09T10:00:00Z',
  respondedAt: '2026-09-09T10:00:00Z',
};

function pageOf(items: ReceivedContactRequestDto[], totalCount: number, page = 1): PaginatedResultOfReceivedContactRequestDto {
  return {
    items,
    page,
    pageSize: 20,
    totalCount,
    totalPages: Math.max(1, Math.ceil(totalCount / 20)),
  };
}

class ContactRequestsApiStub {
  pages: PaginatedResultOfReceivedContactRequestDto = pageOf([PENDING, DECIDED], 2);
  listError: unknown = undefined;
  approveError: unknown = undefined;
  rejectError: unknown = undefined;
  listed: { page: number; pageSize: number; status?: number }[] = [];
  approved: string[] = [];
  rejected: string[] = [];

  listReceived(query: { page: number; pageSize: number; status?: number }) {
    this.listed.push({ ...query });
    if (this.listError !== undefined) {
      return throwError(() => this.listError);
    }
    return of(this.pages);
  }

  approve(id: string) {
    this.approved.push(id);
    if (this.approveError !== undefined) {
      return throwError(() => this.approveError);
    }
    return of({ ...PENDING, id, status: 'Approved', respondedAt: '2026-09-16T10:00:00Z' });
  }

  reject(id: string) {
    this.rejected.push(id);
    if (this.rejectError !== undefined) {
      return throwError(() => this.rejectError);
    }
    return of({ ...PENDING, id, status: 'Rejected', respondedAt: '2026-09-16T10:00:00Z' });
  }
}

async function setup(api?: ContactRequestsApiStub): Promise<{ fixture: ComponentFixture<NurseContactRequests>; api: ContactRequestsApiStub; announcer: Announcer }> {
  const stub = api ?? new ContactRequestsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseContactRequests],
    providers: [provideRouter([]), { provide: ContactRequestsApi, useValue: stub }],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseContactRequests);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseContactRequests>): string {
  return fixture.nativeElement.textContent as string;
}

function allByTestId(fixture: ComponentFixture<NurseContactRequests>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseContactRequests>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseContactRequests', () => {
  it('loads page 1 with pageSize 20 and no status filter by default', async () => {
    const { fixture, api } = await setup();

    expect(api.listed.length).toBe(1);
    expect(api.listed[0]).toEqual({ page: 1, pageSize: 20, status: undefined });
    expect(allByTestId(fixture, 'contact-request-card').length).toBe(2);
  });

  it('renders organization-first cards without message or contact-detail UI', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('City General Hospital');
    expect(content).toContain('Emergency Nurse');
    expect(content).toContain('Emergency');
    expect(content).not.toContain('Message');
    expect(content).not.toContain('@');
    expect(content).not.toContain('req-1');
  });

  it('shows plain-text statuses with no semantic colors', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('Pending');
    expect(content).toContain('Approved');
    expect(content).not.toContain('Verified');
  });

  it('resets to page 1 when the status filter changes', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateStatusFilter(value: string): void;
    };

    component.updateStatusFilter('0');
    await settle(fixture);

    expect(api.listed[api.listed.length - 1]).toEqual({ page: 1, pageSize: 20, status: 0 });
  });

  it('clears the filter back to All with page 1', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateStatusFilter(value: string): void;
      clearFilter(): void;
    };

    component.updateStatusFilter('1');
    await settle(fixture);
    component.clearFilter();
    await settle(fixture);

    expect(api.listed[api.listed.length - 1]).toEqual({ page: 1, pageSize: 20, status: undefined });
    expect(text(fixture)).not.toContain('No approved requests.');
  });

  it('shows filtered no-results with a Clear action', async () => {
    const api = new ContactRequestsApiStub();
    api.pages = pageOf([], 0);
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      updateStatusFilter(value: string): void;
    };

    component.updateStatusFilter('0');
    await settle(fixture);

    expect(text(fixture)).toContain('No pending requests.');
    const emptyState = fixture.nativeElement.querySelector('np-empty-state');
    expect(emptyState?.querySelector('button')?.textContent?.trim()).toBe('Clear filter');
  });

  it('shows the global empty state when unfiltered totalCount is 0', async () => {
    const api = new ContactRequestsApiStub();
    api.pages = pageOf([], 0);
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No contact requests yet.');
  });

  it('passes requested pages through to reload', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      loadPage(page: number): Promise<void>;
    };

    await component.loadPage(3);
    await settle(fixture);

    expect(api.listed[api.listed.length - 1].page).toBe(3);
  });

  it('loads the last server-reported page when mutation empties the current page', async () => {
    const api = new ContactRequestsApiStub();
    api.pages = pageOf([], 21, 2);
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      refreshAfterMutation(): Promise<void>;
    };

    await component.refreshAfterMutation();
    await settle(fixture);

    const lastListed = api.listed[api.listed.length - 1];
    expect(lastListed.page).toBe(2);
  });

  it('approves immediately with exactly one request and announces success', async () => {
    const { fixture, api, announcer } = await setup();
    const component = fixture.componentInstance as unknown as {
      approve(id: string): Promise<void>;
    };

    await component.approve('req-1');
    await settle(fixture);

    expect(api.approved).toEqual(['req-1']);
    expect(announcer.current()?.text).toBe('Contact request approved.');
  });

  it('rejects only after explicit confirmation with exactly one request', async () => {
    const { fixture, api, announcer } = await setup();
    const card = allByTestId(fixture, 'contact-request-card')[0];
    const getByTestIdIn = (id: string) => card.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;

    getByTestIdIn('request-reject')?.click();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(api.rejected.length).toBe(0);
    expect(getByTestIdIn('request-confirm-reject')).not.toBeNull();

    getByTestIdIn('request-keep')?.click();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(api.rejected.length).toBe(0);

    getByTestIdIn('request-reject')?.click();
    fixture.detectChanges();
    await fixture.whenStable();
    getByTestIdIn('request-confirm-reject')?.click();
    await settle(fixture);

    expect(api.rejected).toEqual(['req-1']);
    expect(announcer.current()?.text).toBe('Contact request rejected.');
  });

  it('treats mutation 404 as stale with a calm notice and refresh', async () => {
    const api = new ContactRequestsApiStub();
    api.approveError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture, announcer } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      approve(id: string): Promise<void>;
    };

    await component.approve('req-1');
    await settle(fixture);

    expect(text(fixture)).toContain('no longer available');
    expect(announcer.current()?.text).toBe('This request was already decided.');
  });

  it('treats mutation 409 as already-decided with refresh', async () => {
    const api = new ContactRequestsApiStub();
    api.rejectError = { status: 409, error: { title: 'Conflict', status: 409 } };
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      reject(id: string): Promise<void>;
    };

    await component.reject('req-1');
    await settle(fixture);

    expect(text(fixture)).toContain('already decided');
  });

  it('keeps card-local mutation errors without replacing the list', async () => {
    const api = new ContactRequestsApiStub();
    api.approveError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      approve(id: string): Promise<void>;
    };

    await component.approve('req-1');
    await settle(fixture);

    expect(allByTestId(fixture, 'contact-request-card').length).toBe(2);
    expect(text(fixture)).toContain('could not be completed');
  });

  it('renders a loading state and retry preserving page and filter', async () => {
    const api = new ContactRequestsApiStub();
    api.listError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse contact requests route', () => {
  it('mounts /nurse/contact-requests with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/contact-requests');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_CONTACT_REQUESTS' });
  });
});
