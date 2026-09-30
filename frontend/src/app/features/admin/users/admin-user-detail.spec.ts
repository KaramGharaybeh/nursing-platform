import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AdminUsersApi } from '../../../core/api/admin-users-api';
import type { UserDetailDto } from '../../../core/api/generated/models/user-detail-dto';
import { AdminUserDetail } from './admin-user-detail';

const USER: UserDetailDto = {
  createdAt: '2026-09-14T00:00:00Z',
  email: 'nora@example.com',
  emailVerified: true,
  firstName: 'Nora',
  id: 'user-1',
  isActive: true,
  isProfileComplete: true,
  lastLoginAt: null,
  lastName: 'Nurse',
  permissions: ['Users.View'],
  roles: ['Nurse'],
  username: 'noranurse',
};

class AdminUsersApiStub {
  readonly getCalls: string[] = [];
  readonly updateCalls: { userId: string; roleName: string }[] = [];
  nextUpdateError: unknown;
  get(userId: string) {
    this.getCalls.push(userId);
    return of(USER);
  }
  updateRole(userId: string, request: { roleName: string }) {
    this.updateCalls.push({ userId, roleName: request.roleName });
    if (this.nextUpdateError !== undefined) {
      return throwError(() => this.nextUpdateError);
    }
    return of({ userId, roles: [request.roleName] });
  }
}

async function setup(api = new AdminUsersApiStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [AdminUserDetail],
    providers: [
      provideRouter([]),
      { provide: AdminUsersApi, useValue: api },
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'user-1' } } } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(AdminUserDetail);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return { fixture, api };
}

describe('AdminUserDetail', () => {
  it('loads and displays identifying user information and role choices', async () => {
    const { fixture, api } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(api.getCalls).toEqual(['user-1']);
    expect(text).toContain('Nora Nurse');
    expect(text).toContain('noranurse');
    expect(text).toContain('nora@example.com');
    expect(text).toContain('Current role');
    expect(text).not.toContain('passwordHash');
  });

  it('submits roleName to the protected admin role endpoint and uses backend-confirmed roles', async () => {
    const { fixture, api } = await setup();

    fixture.componentInstance.updateRole('Employer');
    await fixture.componentInstance.saveRole();
    fixture.detectChanges();

    expect(api.updateCalls).toEqual([{ userId: 'user-1', roleName: 'Employer' }]);
    expect(fixture.nativeElement.textContent).toContain('Role updated successfully.');
  });

  it('shows save failures without mutating the role locally', async () => {
    const api = new AdminUsersApiStub();
    api.nextUpdateError = { status: 500, error: { title: 'Error', status: 500, detail: 'Role failed.' } };
    const { fixture } = await setup(api);

    fixture.componentInstance.updateRole('Employer');
    await fixture.componentInstance.saveRole();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Role failed.');
  });
});
