import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of } from 'rxjs';
import { RegisterEmployer } from './register-employer';
import { RegisterEmployerApi } from './register-employer-api';

const meta: Meta<RegisterEmployer> = {
  title: 'Auth/Register Employer',
  component: RegisterEmployer,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: RegisterEmployerApi,
          useValue: { register: () => of(undefined) },
        },
      ],
    }),
  ],
};

export default meta;

type Story = StoryObj<RegisterEmployer>;

export const Default: Story = {};
