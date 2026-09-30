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

export const Success: Story = {
  play: async ({ canvasElement }) => {
    const emailInput = canvasElement.querySelector<HTMLInputElement>('#auth-reset-password-email');
    const newPasswordInput = canvasElement.querySelector<HTMLInputElement>(
      '#auth-reset-password-new-password',
    );
    const submitButton = canvasElement.querySelector<HTMLButtonElement>(
      '.np-reset-password-submit',
    );

    if (emailInput === null || newPasswordInput === null || submitButton === null) {
      throw new Error('Reset password success story could not find the production form controls.');
    }

    emailInput.value = 'nurse@example.com';
    emailInput.dispatchEvent(new Event('input', { bubbles: true }));
    newPasswordInput.value = 'NewPass1x';
    newPasswordInput.dispatchEvent(new Event('input', { bubbles: true }));
    submitButton.click();

    await new Promise((resolve) => setTimeout(resolve, 0));
  },
};

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
