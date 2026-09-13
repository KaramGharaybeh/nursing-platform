import type { Meta, StoryObj } from '@storybook/angular-vite';
import { applicationConfig } from '@storybook/angular-vite';
import { provideRouter } from '@angular/router';
import { RoleSelection } from './role-selection';

const meta: Meta<RoleSelection> = {
  title: 'Auth/Role Selection',
  component: RoleSelection,
  decorators: [applicationConfig({ providers: [provideRouter([])] })],
  parameters: {
    layout: 'fullscreen',
  },
};

export default meta;

type Story = StoryObj<RoleSelection>;

export const Default: Story = {};
