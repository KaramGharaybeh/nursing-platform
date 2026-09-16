import { Component, inject } from '@angular/core';
import type { Meta, StoryObj } from '@storybook/angular-vite';
import { Announcer, type AnnouncementPoliteness } from './announcement';
import { NpLiveRegion } from './live-region';

@Component({
  selector: 'np-live-region-demo',
  imports: [NpLiveRegion],
  template: `
    <np-live-region />
    <p>
      <button type="button" (click)="announce('Experience deleted.', 'polite')">Announce polite</button>
      <button type="button" (click)="announce('Delete failed. Try again.', 'assertive')">
        Announce assertive
      </button>
    </p>
    <p data-testid="np-live-region-demo-mirror">Current: {{ currentText }}</p>
  `,
})
class NpLiveRegionDemo {
  private readonly announcer = inject(Announcer);

  protected get currentText(): string {
    const current = this.announcer.current();
    return current === undefined ? '(none)' : `${current.politeness}: ${current.text}`;
  }

  protected announce(text: string, politeness: AnnouncementPoliteness): void {
    this.announcer.announce(text, politeness);
  }
}

const meta: Meta<NpLiveRegionDemo> = {
  title: 'Shared UI/Live Region',
  component: NpLiveRegionDemo,
};

export default meta;

type Story = StoryObj<NpLiveRegionDemo>;

export const AnnouncementDemo: Story = {};
