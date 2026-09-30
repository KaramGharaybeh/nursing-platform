import type { Meta, StoryObj } from '@storybook/angular-vite';

import { LoadingErrorRetry } from './loading-error-retry';

const meta: Meta<LoadingErrorRetry> = {
  title: 'Shared UI/Loading Error Retry',
  component: LoadingErrorRetry,
};

export default meta;

type Story = StoryObj<LoadingErrorRetry>;

export const Loading: Story = {
  args: {
    state: { kind: 'loading' },
    loadingLabel: 'Loading',
  },
};
