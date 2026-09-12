import { provideHttpClient } from '@angular/common/http';
import { ActivatedRoute, provideRouter } from '@angular/router';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of } from 'rxjs';
import { ResetPassword } from './reset-password';
import { ResetPasswordApi } from './reset-password-api';

function activatedRouteWithToken(token: string | null) {
  return {
    snapshot: {
      queryParamMap: {
        get: (key: string) => (key === 'token' ? token : null),
      },
    },
  };
}

const meta: Meta<ResetPassword> = {
  title: 'Auth/Reset Password',
  component: ResetPassword,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: ResetPasswordApi,
          useValue: { resetPassword: () => of(undefined) },
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteWithToken('storybook-opaque-token'),
        },
      ],
    }),
  ],
};

export default meta;

type Story = StoryObj<ResetPassword>;

export const Default: Story = {};

export const MissingToken: Story = {
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: ResetPasswordApi,
          useValue: { resetPassword: () => of(undefined) },
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteWithToken(null),
        },
      ],
    }),
  ],
};
