import { provideHttpClient } from '@angular/common/http';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of } from 'rxjs';
import { ForgotPassword } from './forgot-password';
import { ForgotPasswordApi } from './forgot-password-api';

const meta: Meta<ForgotPassword> = {
  title: 'Auth/Forgot Password',
  component: ForgotPassword,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        {
          provide: ForgotPasswordApi,
          useValue: { requestPasswordReset: () => of(undefined) },
        },
      ],
    }),
  ],
};

export default meta;

type Story = StoryObj<ForgotPassword>;

export const Default: Story = {};
