import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { provideRouter } from '@angular/router';
import { CheckEmail } from './check-email';

const meta: Meta<CheckEmail> = {
  title: 'Auth/Check Email',
  component: CheckEmail,
  decorators: [applicationConfig({ providers: [provideRouter([])] })],
  parameters: {
    layout: 'fullscreen',
  },
};

export default meta;

type Story = StoryObj<CheckEmail>;

export const Default: Story = {};
