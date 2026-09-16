import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { Announcer } from './announcement';
import { NpLiveRegion } from './live-region';

async function setup(): Promise<{ fixture: ComponentFixture<NpLiveRegion>; announcer: Announcer }> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [NpLiveRegion] }).compileComponents();
  const announcer = TestBed.inject(Announcer);
  const fixture = TestBed.createComponent(NpLiveRegion);
  fixture.detectChanges();
  await fixture.whenStable();
  return { fixture, announcer };
}

function regionElement(fixture: ComponentFixture<NpLiveRegion>): HTMLElement {
  return fixture.nativeElement.querySelector('[data-testid="np-live-region"]') as HTMLElement;
}

describe('NpLiveRegion', () => {
  it('renders a visually hidden polite live region with no announcement initially', async () => {
    const { fixture } = await setup();

    const region = regionElement(fixture);
    expect(region.getAttribute('aria-live')).toBe('polite');
    expect(region.getAttribute('role')).toBe('status');
    expect(region.classList.contains('u-visually-hidden')).toBe(true);
    expect(region.textContent?.trim() ?? '').toBe('');
  });

  it('exposes announced text to assistive technology', async () => {
    const { fixture, announcer } = await setup();

    announcer.announce('Experience deleted.');
    await Promise.resolve();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(regionElement(fixture).textContent).toContain('Experience deleted.');
  });

  it('switches to an assertive alert region for assertive announcements', async () => {
    const { fixture, announcer } = await setup();

    announcer.announce('Delete failed. Try again.', 'assertive');
    await Promise.resolve();
    fixture.detectChanges();
    await fixture.whenStable();

    const region = regionElement(fixture);
    expect(region.getAttribute('aria-live')).toBe('assertive');
    expect(region.getAttribute('role')).toBe('alert');
    expect(region.textContent).toContain('Delete failed. Try again.');
  });

  it('introduces no dialog dependency', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('mat-dialog-container')).toBeNull();
    expect(fixture.nativeElement.innerHTML).not.toContain('mat-datepicker');
  });
});
