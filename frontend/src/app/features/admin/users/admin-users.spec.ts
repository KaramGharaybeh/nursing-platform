import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AdminUsersApi } from '../../../core/api/admin-users-api';
import type { ListUsers$Params } from '../../../core/api/generated/fn/nursing-platform-web-api/list-users';
import type { UserListItemDto } from '../../../core/api/generated/models/user-list-item-dto';
import { AdminUsers } from './admin-users';

const USER: UserListItemDto = {
  createdAt: '2026-09-14T00:00:00Z',
  email: 'nora@example.com',
  emailVerified: true,
  firstName: 'Nora',
  id: 'user-1',
  isActive: true,
  lastLoginAt: null,
  lastName: 'Nurse',
  roles: ['Nurse'],
  username: 'noranurse',
};

class AdminUsersApiStub {
  readonly listCalls: (ListUsers$Params | undefined)[] = [];
  nextError: unknown;
  result = { items: [USER], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 };
  list(params?: ListUsers$Params) {
    this.listCalls.push(params);
    if (this.nextError !== undefined) {
      return throwError(() => this.nextError);
    }
    return of(this.result);
  }
}

async function setup(api = new AdminUsersApiStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [AdminUsers],
    providers: [provideRouter([]), { provide: AdminUsersApi, useValue: api }],
  }).compileComponents();
  const fixture = TestBed.createComponent(AdminUsers);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return { fixture, api };
}

describe('AdminUsers', () => {
  it('loads typed user results and displays identifying fields without permissions', async () => {
    const { fixture, api } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(api.listCalls[0]).toMatchObject({ page: 1, pageSize: 10, sort: 'name' });
    expect(text).toContain('Nora Nurse');
    expect(text).toContain('noranurse');
    expect(text).toContain('nora@example.com');
    expect(text).toContain('Nurse');
    expect(text).not.toContain('passwordHash');
  });

  it('sends search to the server instead of filtering only in the browser', async () => {
    const { fixture, api } = await setup();
    const search = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('#admin-users-search');
    if (search === null) {
      throw new Error('Missing admin user search input');
    }
    search.value = 'noranurse';
    fixture.componentInstance.updateSearch('noranurse');
    fixture.detectChanges();

    await fixture.componentInstance.submitSearch();

    expect(api.listCalls.at(-1)).toMatchObject({ page: 1, pageSize: 10, search: 'noranurse' });
  });

  it('renders an error state when user search fails', async () => {
    const api = new AdminUsersApiStub();
    api.nextError = { status: 500, error: { title: 'Users failed', status: 500, detail: 'Try later.' } };

    const { fixture } = await setup(api);

    expect(fixture.nativeElement.textContent).toContain('Users failed');
    expect(fixture.nativeElement.textContent).toContain('Try later.');
  });
});
