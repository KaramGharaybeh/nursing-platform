import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { NpEmptyState } from './empty-state';

async function setup(inputs: {
  kind?: 'empty' | 'no-results';
  title?: string;
  description?: string;
  actionLabel?: string;
}): Promise<ComponentFixture<NpEmptyState>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [NpEmptyState] }).compileComponents();
  const fixture = TestBed.createComponent(NpEmptyState);
  fixture.componentRef.setInput('kind', inputs.kind ?? 'empty');
  fixture.componentRef.setInput('title', inputs.title ?? 'No skills added yet.');
  if (inputs.description !== undefined) {
    fixture.componentRef.setInput('description', inputs.description);
  }
  if (inputs.actionLabel !== undefined) {
    fixture.componentRef.setInput('actionLabel', inputs.actionLabel);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

describe('NpEmptyState', () => {
  it('renders an empty heading and copy without implying a filtered state', async () => {
    const fixture = await setup({ kind: 'empty', description: 'Add your first skill to get started.' });
    const content = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelector('h2')?.textContent).toContain('No skills added yet.');
    expect(content).toContain('Add your first skill to get started.');
    expect(content).not.toContain('filter');
    expect(content).not.toContain('search');
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
  });

  it('renders an accessible primary action only when a label is supplied', async () => {
    const fixture = await setup({ kind: 'empty', actionLabel: 'Add skill' });
    const component = fixture.componentInstance;
    let emissions = 0;
    component.actionRequested.subscribe(() => {
      emissions += 1;
    });

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement | null;
    expect(button?.textContent?.trim()).toBe('Add skill');
    expect(button?.type).toBe('button');
    button?.click();

    expect(emissions).toBe(1);
  });

  it('renders a distinct no-results mode that never claims a global empty', async () => {
    const fixture = await setup({
      kind: 'no-results',
      title: 'No users match your search.',
      description: 'Try a different name or clear the search.',
      actionLabel: 'Clear search',
    });
    const content = fixture.nativeElement.textContent as string;

    expect(fixture.nativeElement.querySelector('h2')?.textContent).toContain('No users match your search.');
    expect(content).toContain('Try a different name or clear the search.');
    expect(content).not.toContain('No users are available yet.');
    expect(fixture.nativeElement.querySelector('button')?.textContent?.trim()).toBe('Clear search');
  });

  it('emits the no-results action exactly once without mutating consumer state', async () => {
    const fixture = await setup({ kind: 'no-results', title: 'No results.', actionLabel: 'Clear filters' });
    const component = fixture.componentInstance;
    let emissions = 0;
    component.actionRequested.subscribe(() => {
      emissions += 1;
    });

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();

    expect(emissions).toBe(2);
  });

  it('requires no illustration or icon to convey meaning', async () => {
    const fixture = await setup({ kind: 'empty' });
    const html = fixture.nativeElement.innerHTML as string;

    expect(html).not.toContain('<svg');
    expect(html).not.toContain('<img');
    expect(html).not.toContain('mat-icon');
  });

  it('keeps an accessible landmark structure with logical styling hooks', async () => {
    const fixture = await setup({ kind: 'no-results', title: 'Nothing here.' });
    const section = fixture.nativeElement.querySelector('section') as HTMLElement | null;

    expect(section?.getAttribute('aria-labelledby')).toContain('np-empty-state-title');
    expect(fixture.nativeElement.querySelector('h2')?.id).toContain('np-empty-state-title');
  });

  it('does not announce static content through live regions', async () => {
    const fixture = await setup({ kind: 'empty' });
    const html = fixture.nativeElement.innerHTML as string;

    expect(html).not.toContain('aria-live');
    expect(html).not.toContain('role="alert"');
    expect(html).not.toContain('role="status"');
  });
});
