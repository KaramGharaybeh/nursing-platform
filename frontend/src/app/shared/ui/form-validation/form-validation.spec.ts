import { ComponentFixture, TestBed } from '@angular/core/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';
import { NpFormValidationSummary } from './form-validation-summary';
import {
  toFieldErrorText,
  toFormValidationItems,
  toFormValidationSummary,
} from './form-validation';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const validationDir = 'src/app/shared/ui/form-validation';

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function validationError(overrides: Partial<NormalizedProblemDetails> = {}): NormalizedProblemDetails {
  return Object.freeze({
    kind: 'validation',
    type: 'https://httpstatuses.com/400',
    title: 'Validation failed',
    status: 400,
    detail: 'One or more validation errors occurred.',
    traceId: 'trace-validation-1',
    errors: {
      Email: ['Email is required.'],
      Password: ['Enter a value.', 'Use at least 8 characters.'],
    },
    ...overrides,
  }) as NormalizedProblemDetails;
}

function query<T extends Element>(fixture: ComponentFixture<NpFormValidationSummary>, selector: string): T | null {
  return fixture.nativeElement.querySelector(selector) as T | null;
}

function all<T extends Element>(fixture: ComponentFixture<NpFormValidationSummary>, selector: string): T[] {
  return Array.from(fixture.nativeElement.querySelectorAll(selector)) as T[];
}

async function setupSummary(): Promise<ComponentFixture<NpFormValidationSummary>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({ imports: [NpFormValidationSummary] }).compileComponents();
  const fixture = TestBed.createComponent(NpFormValidationSummary);
  fixture.componentRef.setInput('summaryTitle', 'Check the highlighted fields');
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

describe('form validation presentation pattern', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('converts field message lists into single actionable inline error text', () => {
    expect(toFieldErrorText(['Email is required.'])).toBe('Email is required.');
    expect(toFieldErrorText(['Enter a value.', 'Use at least 8 characters.'])).toBe(
      'Enter a value. Use at least 8 characters.',
    );
    expect(toFieldErrorText(['  ', ''])).toBe('');
    expect(toFieldErrorText(undefined)).toBe('');
    expect(toFieldErrorText([])).toBe('');
  });

  it('maps normalized validation errors to deterministic summary items with caller labels', () => {
    const items = toFormValidationItems(validationError().errors, {
      fieldLabels: { Email: 'Email address', Password: 'Password' },
      controlIds: { Email: 'email-field', Password: 'password-field' },
    });

    expect(items.map((item) => item.fieldKey)).toEqual(['Email', 'Password']);
    expect(items[0]?.label).toBe('Email address');
    expect(items[0]?.controlId).toBe('email-field');
    expect(items[0]?.message).toBe('Email is required.');
    expect(items[1]?.label).toBe('Password');
    expect(items[1]?.controlId).toBe('password-field');
    expect(items[1]?.message).toBe('Enter a value. Use at least 8 characters.');
  });

  it('sanitizes unsafe server keys and never exposes trace, code, or raw detail', () => {
    const normalized = validationError({
      code: 'validation-failed' as never,
      traceId: 'trace-secret-1',
      detail: '<script>alert(1)</script>',
      errors: {
        '<img src=x onerror=alert(1)>': ['Enter a value.'],
      },
    });
    const summary = toFormValidationSummary(normalized, {
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The form could not be submitted.',
    });
    const displayed = JSON.stringify({
      title: summary.title,
      items: summary.items.map((item) => ({ label: item.label, controlId: item.controlId, message: item.message })),
      formErrors: summary.formErrors,
    });

    expect(summary.hasErrors).toBe(true);
    expect(summary.items).toHaveLength(1);
    expect(displayed).not.toContain('<script');
    expect(displayed).not.toContain('<img');
    expect(displayed).not.toContain('trace-secret-1');
    expect(displayed).not.toContain('validation-failed');
    expect(summary.items[0]?.label).not.toContain('<');
    expect(summary.items[0]?.label).not.toContain('>');
  });

  it('uses caller-provided safe fallback for non-validation errors without raw server content', () => {
    const generic: NormalizedProblemDetails = Object.freeze({
      kind: 'generic',
      type: 'https://httpstatuses.com/500',
      title: 'Server failure <b>raw</b>',
      status: 500,
      detail: 'Trace trace-secret-2 with code internal-boom.',
      traceId: 'trace-secret-2',
    }) as NormalizedProblemDetails;

    const summary = toFormValidationSummary(generic, {
      summaryTitle: 'Check the highlighted fields',
      formErrorFallback: 'The form could not be submitted.',
    });
    const raw = JSON.stringify(summary);

    expect(summary.hasErrors).toBe(true);
    expect(summary.items).toEqual([]);
    expect(summary.formErrors).toEqual(['The form could not be submitted.']);
    expect(raw).not.toContain('trace-secret-2');
    expect(raw).not.toContain('Server failure');
    expect(raw).not.toContain('internal-boom');
  });

  it('renders a persistent summary that complements inline errors with programmatic field links', async () => {
    const fixture = await setupSummary();
    fixture.componentRef.setInput(
      'items',
      toFormValidationItems(validationError().errors, {
        fieldLabels: { Email: 'Email address', Password: 'Password' },
        controlIds: { Email: 'email-field', Password: 'password-field' },
      }),
    );
    fixture.detectChanges();
    await fixture.whenStable();

    const section = query<HTMLElement>(fixture, 'section.np-form-validation-summary');
    const links = all<HTMLAnchorElement>(fixture, '.np-form-validation-summary-list a');

    expect(section?.getAttribute('role')).toBe('alert');
    expect(section?.getAttribute('tabindex')).toBe('-1');
    expect(fixture.nativeElement.textContent as string).toContain('Check the highlighted fields');
    expect(links).toHaveLength(2);
    expect(links[0]?.getAttribute('href')).toBe('#email-field');
    expect(links[0]?.textContent).toContain('Email address');
    expect(links[0]?.textContent).toContain('Email is required.');
    expect(query(fixture, 'input')).toBeNull();
  });

  it('exposes programmatic focus without stealing focus or clearing caller values', async () => {
    const fixture = await setupSummary();
    const items = toFormValidationItems(validationError().errors, {
      fieldLabels: { Email: 'Email address' },
      controlIds: { Email: 'email-field' },
    });
    const snapshot = JSON.stringify(items);
    fixture.componentRef.setInput('items', items);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(typeof fixture.componentInstance.focusSummary).toBe('function');
    expect(document.activeElement).not.toBe(query(fixture, 'section.np-form-validation-summary'));
    expect(JSON.stringify(items)).toBe(snapshot);
  });

  it('keeps presentation free of timing, business, routing, auth, and generated behavior', () => {
    const helperSource = readTextFile(`${validationDir}/form-validation.ts`).toLowerCase();
    const componentSource = readTextFile(`${validationDir}/form-validation-summary.ts`).toLowerCase();
    const templateSource = readTextFile(`${validationDir}/form-validation-summary.html`).toLowerCase();
    const combined = `${helperSource}\n${componentSource}\n${templateSource}`;

    expect(helperSource).toContain('normalizedproblemdetails');
    expect(combined).not.toContain('touched');
    expect(combined).not.toContain('dirty');
    expect(combined).not.toContain('formcontrol');
    expect(combined).not.toContain('formgroup');
    expect(combined).not.toContain('validators');
    expect(combined).not.toContain('httpclient');
    expect(combined).not.toContain('router');
    expect(combined).not.toContain('navigate');
    expect(combined).not.toContain('auth');
    expect(combined).not.toContain('login');
    expect(combined).not.toContain('register');
    expect(combined).not.toContain('password must');
    expect(combined).not.toContain('generated');
    expect(combined).not.toContain('settimeout');
    expect(combined).not.toContain('setinterval');
  });

  it('uses external template and stylesheet with logical layout and no invented colors', () => {
    const componentSource = readTextFile(`${validationDir}/form-validation-summary.ts`);
    const stylesSource = readTextFile(`${validationDir}/form-validation-summary.scss`);
    const templateSource = readTextFile(`${validationDir}/form-validation-summary.html`);

    expect(componentSource).toContain("templateUrl: './form-validation-summary.html'");
    expect(componentSource).toContain("styleUrl: './form-validation-summary.scss'");
    expect(componentSource).not.toMatch(/template\s*:/);
    expect(componentSource).not.toMatch(/styles\s*:/);
    expect(templateSource).not.toContain('<style');
    expect(stylesSource).toContain('text-align: start');
    expect(stylesSource).not.toMatch(/#[0-9A-Fa-f]{3,8}\b/);
    expect(stylesSource).not.toContain('margin-left');
    expect(stylesSource).not.toContain('margin-right');
    expect(stylesSource).not.toContain('padding-left');
    expect(stylesSource).not.toContain('padding-right');
  });
});
