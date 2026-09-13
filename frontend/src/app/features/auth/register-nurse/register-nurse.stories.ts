import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { of } from 'rxjs';
import { RegisterNurse } from './register-nurse';
import { RegisterNurseApi } from './register-nurse-api';

const meta: Meta<RegisterNurse> = {
  title: 'Auth/Register Nurse',
  component: RegisterNurse,
  parameters: {
    layout: 'fullscreen',
  },
  decorators: [
    applicationConfig({
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: RegisterNurseApi,
          useValue: { register: () => of(undefined) },
        },
      ],
    }),
  ],
};

export default meta;

type Story = StoryObj<RegisterNurse>;

export const Default: Story = {};
