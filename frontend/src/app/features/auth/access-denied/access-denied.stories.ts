import type { Meta, StoryObj } from '@storybook/angular-vite';
import { AccessDenied } from './access-denied';

const meta: Meta<AccessDenied> = {
  title: 'Auth/Access Denied',
  component: AccessDenied,
  parameters: {
    layout: 'fullscreen',
  },
};

export default meta;

type Story = StoryObj<AccessDenied>;

export const Default: Story = {};
