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

describe('AdminUserDetail - form submission', () => {
  it('updateRole is called when select emits valueChange', async () => {
    const { fixture } = await setup();

    // Find the select control
    const selectControl = fixture.debugElement.query(de => de.name === 'np-select-control');
    if (!selectControl) {
      throw new Error('Missing np-select-control');
    }

    // Simulate user selecting Employer
    selectControl.componentInstance.valueChange.emit('Employer');
    fixture.detectChanges();

    // Verify roleControl was updated
    expect(fixture.componentInstance['roleControl'].value).toBe('Employer');
  });

  it('clicking Save role submits the selected role to the API once', async () => {
    const { fixture, api } = await setup();

    // Set role to Employer via updateRole
    fixture.componentInstance.updateRole('Employer');
    fixture.detectChanges();

    const saveButton = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('button');
    if (saveButton === null) {
      throw new Error('Missing Save role button');
    }

    saveButton.click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(api.updateCalls).toEqual([{ userId: 'user-1', roleName: 'Employer' }]);
    expect(fixture.nativeElement.textContent).toContain('Role updated successfully.');
  });
});
