import type { Meta, StoryObj } from '@storybook/angular-vite';
import { NpPagination } from './pagination';

const meta: Meta<NpPagination> = {
  title: 'Shared UI/Pagination',
  component: NpPagination,
};

export default meta;

type Story = StoryObj<NpPagination>;

const baseArgs = {
  totalPages: 4,
  totalCount: 38,
  itemLabel: 'requests',
  ariaLabel: 'Contact requests pagination',
};

export const FirstPage: Story = {
  args: { ...baseArgs, page: 1 },
};

export const MiddlePage: Story = {
  args: { ...baseArgs, page: 2 },
};

export const LastPage: Story = {
  args: { ...baseArgs, page: 4 },
};

export const SinglePage: Story = {
  args: { ...baseArgs, page: 1, totalPages: 1, totalCount: 7 },
};

export const NarrowWrap: Story = {
  args: { ...baseArgs, page: 3 },
  decorators: [
    (story) => {
      const inner = story();
      return {
        ...inner,
        template: `<div style="max-inline-size: 320px; border: 1px dashed currentColor; padding: 8px;">${inner.template}</div>`,
      };
    },
  ],
};
