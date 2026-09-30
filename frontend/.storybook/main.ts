import type { StorybookConfig } from '@storybook/angular-vite';

const config: StorybookConfig = {
  stories: ['../src/**/*.stories.@(ts|tsx)'],
  framework: {
    name: '@storybook/angular-vite',
    options: {},
  },
  async viteFinal(config) {
    return {
      ...config,
      css: {
        ...config.css,
        preprocessorOptions: {
          ...config.css?.preprocessorOptions,
          scss: {
            ...config.css?.preprocessorOptions?.scss,
            loadPaths: ['src'],
          },
        },
      },
    };
  },
};

export default config;
