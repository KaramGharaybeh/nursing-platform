import type { Meta, StoryObj } from '@storybook/angular-vite';
import { NpEmptyState } from './empty-state';

const meta: Meta<NpEmptyState> = {
  title: 'Shared UI/Empty State',
  component: NpEmptyState,
};

export default meta;

type Story = StoryObj<NpEmptyState>;

export const Empty: Story = {
  args: {
    kind: 'empty',
    title: 'No certificates added yet.',
  },
};

export const EmptyWithAction: Story = {
  args: {
    kind: 'empty',
    title: 'No positions added yet.',
    description: 'Add your employment history to keep your professional profile up to date.',
    actionLabel: 'Add experience',
  },
};

export const NoResults: Story = {
  args: {
    kind: 'no-results',
    title: 'No users match your search.',
    description: 'Try a different name or clear the search to see everyone.',
    actionLabel: 'Clear search',
  },
};
