import type { Meta, StoryObj } from '@storybook/angular-vite';
import { SessionExpired } from './session-expired';

const meta: Meta<SessionExpired> = {
  title: 'Auth/Session Expired',
  component: SessionExpired,
  parameters: {
    layout: 'fullscreen',
  },
};

export default meta;

type Story = StoryObj<SessionExpired>;

export const Default: Story = {};
