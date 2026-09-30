import { provideHttpClient } from '@angular/common/http';
import { ActivatedRoute, provideRouter } from '@angular/router';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of, throwError } from 'rxjs';
import { VerifyEmail } from './verify-email';
import { VerifyEmailApi } from './verify-email-api';

function activatedRouteWithToken(token: string | null) {
  return {
    snapshot: {
      queryParamMap: {
        get: (key: string) => (key === 'token' ? token : null),
      },
    },
  };
}

const meta: Meta<VerifyEmail> = {
  title: 'Auth/Verify Email',
  component: VerifyEmail,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: VerifyEmailApi,
          useValue: { verifyEmail: () => of(undefined) },
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

type Story = StoryObj<VerifyEmail>;

export const Default: Story = {};

export const MissingToken: Story = {
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: VerifyEmailApi,
          useValue: { verifyEmail: () => of(undefined) },
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteWithToken(null),
        },
      ],
    }),
  ],
};

export const VerificationError: Story = {
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: VerifyEmailApi,
          useValue: {
            verifyEmail: () =>
              throwError(() => ({
                status: 409,
                error: {
                  title: 'Conflict',
                  status: 409,
                  detail: 'Verification token has expired.',
                },
              })),
          },
        },
        {
          provide: ActivatedRoute,
          useValue: activatedRouteWithToken('storybook-opaque-token'),
        },
      ],
    }),
  ],
};
