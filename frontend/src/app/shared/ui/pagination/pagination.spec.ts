import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { NpPagination, resolveListState, type NpListState } from './pagination';

async function setup(inputs: {
  page?: number;
  totalPages?: number;
  totalCount?: number;
  itemLabel?: string;
  ariaLabel?: string;
}): Promise<ComponentFixture<NpPagination>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [NpPagination] }).compileComponents();
  const fixture = TestBed.createComponent(NpPagination);
  fixture.componentRef.setInput('page', inputs.page ?? 1);
  fixture.componentRef.setInput('totalPages', inputs.totalPages ?? 1);
  fixture.componentRef.setInput('totalCount', inputs.totalCount ?? 0);
  if (inputs.itemLabel !== undefined) {
    fixture.componentRef.setInput('itemLabel', inputs.itemLabel);
  }
  if (inputs.ariaLabel !== undefined) {
    fixture.componentRef.setInput('ariaLabel', inputs.ariaLabel);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function previousButton(fixture: ComponentFixture<NpPagination>): HTMLButtonElement | null {
  return fixture.nativeElement.querySelector('[data-testid="pagination-previous"]');
}

function nextButton(fixture: ComponentFixture<NpPagination>): HTMLButtonElement | null {
  return fixture.nativeElement.querySelector('[data-testid="pagination-next"]');
}

describe('NpPagination', () => {
  it('does not render navigation when totalPages is 1 or less', async () => {
    for (const totalPages of [0, 1]) {
      const fixture = await setup({ page: 1, totalPages, totalCount: 0 });

      expect(fixture.nativeElement.querySelector('nav')).toBeNull();
    }
  });

  it('disables Previous on the first page and enables Next before the last page', async () => {
    const fixture = await setup({ page: 1, totalPages: 4, totalCount: 38 });

    expect(previousButton(fixture)?.disabled).toBe(true);
    expect(nextButton(fixture)?.disabled).toBe(false);
  });

  it('enables Previous after the first page and disables Next on the last page', async () => {
    const fixture = await setup({ page: 4, totalPages: 4, totalCount: 38 });

    expect(previousButton(fixture)?.disabled).toBe(false);
    expect(nextButton(fixture)?.disabled).toBe(true);
  });

  it('emits the exact 1-based previous and next pages', async () => {
    const fixture = await setup({ page: 2, totalPages: 4, totalCount: 38 });
    const component = fixture.componentInstance;
    const emitted: number[] = [];
    component.pageRequested.subscribe((page: number) => {
      emitted.push(page);
    });

    previousButton(fixture)?.click();
    nextButton(fixture)?.click();

    expect(emitted).toEqual([1, 3]);
  });

  it('never emits out-of-range pages from disabled controls', async () => {
    const first = await setup({ page: 1, totalPages: 3, totalCount: 25 });
    const firstComponent = first.componentInstance;
    let firstEmissions = 0;
    firstComponent.pageRequested.subscribe(() => {
      firstEmissions += 1;
    });
    previousButton(first)?.click();
    expect(firstEmissions).toBe(0);

    const last = await setup({ page: 3, totalPages: 3, totalCount: 25 });
    const lastComponent = last.componentInstance;
    let lastEmissions = 0;
    lastComponent.pageRequested.subscribe(() => {
      lastEmissions += 1;
    });
    nextButton(last)?.click();
    expect(lastEmissions).toBe(0);
  });

  it('shows the current page, total pages, and total count as plain text', async () => {
    const fixture = await setup({ page: 2, totalPages: 4, totalCount: 38, itemLabel: 'requests' });
    const content = fixture.nativeElement.textContent as string;

    expect(content).toContain('2');
    expect(content).toContain('4');
    expect(content).toContain('38');
    expect(content).toContain('requests');
    expect(content).toContain('Page 2 of 4 · 38 requests');
  });

  it('exposes a nav landmark with an accessible label and native button semantics', async () => {
    const fixture = await setup({ page: 2, totalPages: 4, totalCount: 38, ariaLabel: 'Contact requests pagination' });
    const nav = fixture.nativeElement.querySelector('nav') as HTMLElement | null;

    expect(nav?.getAttribute('aria-label')).toBe('Contact requests pagination');
    expect(previousButton(fixture)?.tagName).toBe('BUTTON');
    expect(nextButton(fixture)?.tagName).toBe('BUTTON');
    expect(previousButton(fixture)?.textContent?.trim()).toBe('Previous');
    expect(nextButton(fixture)?.textContent?.trim()).toBe('Next');
  });

  it('holds no API, router, or filter dependencies', async () => {
    const fixture = await setup({ page: 2, totalPages: 4, totalCount: 38 });
    const html = fixture.nativeElement.innerHTML as string;

    expect(html).not.toContain('mat-paginator');
    const component = fixture.componentInstance as unknown as Record<string, unknown>;
    expect(component['router']).toBeUndefined();
    expect(component['http']).toBeUndefined();
  });
});

describe('resolveListState', () => {
  it.each([
    [{ hasItems: true, hasActiveQuery: false }, 'results'],
    [{ hasItems: true, hasActiveQuery: true }, 'results'],
    [{ hasItems: false, hasActiveQuery: false }, 'empty'],
    [{ hasItems: false, hasActiveQuery: true }, 'no-results'],
  ] as const)('maps %o to %s', (input, expected: NpListState) => {
    expect(resolveListState(input.hasItems, input.hasActiveQuery)).toBe(expected);
  });
});
