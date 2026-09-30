import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { CurrentUserStore } from '../../core/auth/current-user-store';
import { ProfileApi } from '../../core/api/profile-api';
import { Account } from './account';

const VERIFIED_USER = {
  id: 'user-1',
  email: 'nurse@example.com',
  username: 'nurseuser',
  firstName: 'Nadia',
  lastName: 'Nursefield',
  isProfileComplete: true,
  isActive: true,
  emailVerified: true,
  roles: ['Nurse'],
  permissions: [],
  createdAt: '2026-09-01T10:00:00Z',
  lastLoginAt: undefined,
};

class StoreStub {
  constructor(private readonly user: typeof VERIFIED_USER | undefined = VERIFIED_USER) {}

  currentUser() {
    return this.user;
  }
}

class ProfileApiStub {
  updateCurrentUserProfile(body: unknown) {
    const request = body as { firstName: string; lastName: string };
    return of({ username: 'nurseuser', ...request, isProfileComplete: true });
  }
}

function providers(user: typeof VERIFIED_USER | undefined) {
  return [
    provideRouter([]),
    { provide: CurrentUserStore, useValue: new StoreStub(user) },
    { provide: ProfileApi, useValue: new ProfileApiStub() },
  ];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<Account> = {
  component: Account,
  title: 'Features/Account/Overview',
};
export default meta;
type Story = StoryObj<Account>;

export const Loaded: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(VERIFIED_USER) })],
};

export const NotVerified: Story = {
  decorators: [
    (story) => ({ ...story(), providers: providers({ ...VERIFIED_USER, emailVerified: false }) }),
  ],
};

export const EditPersonalDetails: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(VERIFIED_USER) })],
  play: async ({ canvasElement }) => {
    await settle();
    const edit = canvasElement.querySelector<HTMLElement>('[data-testid="edit-personal-details"]');
    if (edit === null) {
      throw new Error('Account edit story could not find the Edit personal details action.');
    }
    edit.click();
    await settle();
  },
};

export const ValidationErrors: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(VERIFIED_USER) })],
  play: async ({ canvasElement }) => {
    await settle();
    const edit = canvasElement.querySelector<HTMLElement>('[data-testid="edit-personal-details"]');
    if (edit === null) {
      throw new Error('Account validation story could not find the Edit personal details action.');
    }
    edit.click();
    await settle();
    const firstName = canvasElement.querySelector<HTMLInputElement>('#account-personal-details-first-name');
    if (firstName === null) {
      throw new Error('Account validation story could not find the first-name input.');
    }
    firstName.value = '';
    firstName.dispatchEvent(new Event('input', { bubbles: true }));
    const save = Array.from(canvasElement.querySelectorAll('button')).find(
      (button) => button.textContent?.trim() === 'Save',
    );
    if (save === undefined) {
      throw new Error('Account validation story could not find the Save action.');
    }
    save.click();
    await settle();
  },
};
