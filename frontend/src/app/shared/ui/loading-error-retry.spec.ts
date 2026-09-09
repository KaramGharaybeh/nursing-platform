import { ComponentFixture, TestBed } from '@angular/core/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import type { NormalizedProblemDetails } from '../../core/api/problem-details';
import { LoadingErrorRetry, type LoadingErrorRetryState } from './loading-error-retry';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function normalizedError(overrides: Partial<NormalizedProblemDetails> = {}): NormalizedProblemDetails {
  return Object.freeze({
    kind: 'generic',
    type: 'https://httpstatuses.com/503',
    title: 'Service unavailable',
    status: 503,
    detail: 'Please try again later.',
    traceId: 'trace-shared-1',
    ...overrides,
  });
}

async function setup(): Promise<ComponentFixture<LoadingErrorRetry>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [LoadingErrorRetry] }).compileComponents();
  const fixture = TestBed.createComponent(LoadingErrorRetry);
  fixture.componentRef.setInput('loadingLabel', 'Loading content');
  fixture.componentRef.setInput('fallbackErrorTitle', 'Unable to load content');
  fixture.componentRef.setInput('retryLabel', 'Try again');
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function textContent(fixture: ComponentFixture<LoadingErrorRetry>): string {
  return fixture.nativeElement.textContent as string;
}

function query<T extends Element>(fixture: ComponentFixture<LoadingErrorRetry>, selector: string): T | null {
  return fixture.nativeElement.querySelector(selector) as T | null;
}

async function renderState(
  fixture: ComponentFixture<LoadingErrorRetry>,
  state: LoadingErrorRetryState,
): Promise<void> {
  fixture.componentRef.setInput('state', state);
  fixture.detectChanges();
  await fixture.whenStable();
}

describe('LoadingErrorRetry', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders loading state with perceivable status semantics and text', async () => {
    const fixture = await setup();
    await renderState(fixture, { kind: 'loading' });

    const status = query<HTMLElement>(fixture, '[role="status"]');

    expect(status).not.toBeNull();
    expect(status?.getAttribute('aria-live')).toBe('polite');
    expect(status?.getAttribute('aria-busy')).toBe('true');
    expect(textContent(fixture)).toContain('Loading content');
    expect(query(fixture, '[role="alert"]')).toBeNull();
    expect(query(fixture, 'button')).toBeNull();
  });

  it('renders normalized error title and optional detail without raw generated DTO coupling', async () => {
    const fixture = await setup();
    await renderState(fixture, {
      kind: 'error',
      error: normalizedError({ title: 'Request failed', detail: 'The server rejected the request.' }),
      canRetry: false,
    });

    const alert = query<HTMLElement>(fixture, '[role="alert"]');

    expect(alert).not.toBeNull();
    expect(alert?.getAttribute('aria-live')).toBe('assertive');
    expect(textContent(fixture)).toContain('Request failed');
    expect(textContent(fixture)).toContain('The server rejected the request.');
    expect(query(fixture, 'button')).toBeNull();
  });

  it('uses fallback title and omits detail when normalized error fields are missing', async () => {
    const fixture = await setup();
    await renderState(fixture, {
      kind: 'error',
      error: normalizedError({ title: '', detail: '' }),
      canRetry: false,
    });

    expect(textContent(fixture)).toContain('Unable to load content');
    expect(query(fixture, '.np-loading-error-retry-detail')).toBeNull();
  });

  it('emits retry only after explicit user action and never automatically', async () => {
    const fixture = await setup();
    let retryCount = 0;
    fixture.componentInstance.retryRequested.subscribe(() => {
      retryCount += 1;
    });
    await renderState(fixture, {
      kind: 'error',
      error: normalizedError(),
      canRetry: true,
    });

    const button = query<HTMLButtonElement>(fixture, 'button');

    expect(button).not.toBeNull();
    expect(button?.type).toBe('button');
    expect(button?.textContent).toContain('Try again');
    expect(retryCount).toBe(0);

    button?.click();
    fixture.detectChanges();

    expect(retryCount).toBe(1);
  });

  it('keeps retry control keyboard usable through a native button', async () => {
    const fixture = await setup();
    let retryCount = 0;
    fixture.componentInstance.retryRequested.subscribe(() => {
      retryCount += 1;
    });
    await renderState(fixture, {
      kind: 'error',
      error: normalizedError(),
      canRetry: true,
    });

    const button = query<HTMLButtonElement>(fixture, 'button');

    button?.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    button?.click();
    fixture.detectChanges();

    expect(button?.tagName).toBe('BUTTON');
    expect(retryCount).toBe(1);
  });

  it('renders ready state as projected shared content without empty-state behavior', async () => {
    const fixture = await setup();
    await renderState(fixture, { kind: 'ready' });

    expect(query(fixture, '[role="status"]')).toBeNull();
    expect(query(fixture, '[role="alert"]')).toBeNull();
    expect(query(fixture, 'button')).toBeNull();
  });

  it('keeps implementation free of feature copy, routing, auth, generated, and business behavior', () => {
    const componentSource = readTextFile('src/app/shared/ui/loading-error-retry.ts').toLowerCase();
    const stylesSource = readTextFile('src/app/shared/ui/loading-error-retry.scss').toLowerCase();
    const combined = `${componentSource}\n${stylesSource}`;

    expect(componentSource).toContain('normalizedproblemdetails');
    expect(combined).not.toContain('router');
    expect(combined).not.toContain('navigate');
    expect(combined).not.toContain('auth');
    expect(combined).not.toContain('logout');
    expect(combined).not.toContain('generated');
    expect(combined).not.toContain('httpclient');
    expect(combined).not.toContain('exam');
    expect(combined).not.toContain('nurse');
    expect(combined).not.toContain('payment');
    expect(combined).not.toContain('preparation');
    expect(combined).not.toContain('empty');
    expect(combined).not.toContain('settimeout');
    expect(combined).not.toContain('interval');
  });
});
