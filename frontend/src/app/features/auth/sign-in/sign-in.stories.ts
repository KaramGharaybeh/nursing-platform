import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of } from 'rxjs';
import { AuthTransport } from '../../../core/api/auth-transport';
import type { AuthResult } from '../../../core/api/generated/models/auth-result';
import { AuthSessionBootstrap } from '../../../core/auth/auth-session-bootstrap';
import { CurrentUserStore } from '../../../core/auth/current-user-store';
import { SignIn } from './sign-in';

const authResult: AuthResult = {
  accessToken: 'storybook-access-token',
  expiresAt: '2026-09-08T12:00:00Z',
  refreshToken: 'storybook-refresh-token',
};

const meta: Meta<SignIn> = {
  title: 'Auth/Sign In',
  component: SignIn,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: AuthTransport,
          useValue: { login: () => of(authResult) },
        },
        {
          provide: AuthSessionBootstrap,
          useValue: { establishAuthenticatedSession: () => 'authenticated' },
        },
        {
          provide: CurrentUserStore,
          useValue: { hydrate: () => of('ready') },
        },
      ],
    }),
  ],
};

export default meta;

type Story = StoryObj<SignIn>;

export const Default: Story = {};
