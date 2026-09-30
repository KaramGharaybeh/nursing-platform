import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MatSelect } from '@angular/material/select';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import {
  NpCheckboxControl,
  NpDateControl,
  NpRadioGroupControl,
  NpSelectControl,
  NpTextareaControl,
  NpTextInputControl,
  type NpSelectOption,
} from './standard-form-controls';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const formControlsDir = 'src/app/shared/ui/form-controls';

function readText(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function query<T extends Element>(fixture: ComponentFixture<unknown>, selector: string): T | null {
  return fixture.nativeElement.querySelector(selector) as T | null;
}

function all<T extends Element>(fixture: ComponentFixture<unknown>, selector: string): T[] {
  return Array.from(fixture.nativeElement.querySelectorAll(selector)) as T[];
}

@Component({
  imports: [NpTextInputControl, NpTextareaControl, NpSelectControl, NpCheckboxControl, NpRadioGroupControl, NpDateControl],
  template: `
    <np-text-input-control
      label="Email address"
      controlId="email-field"
      type="email"
      value="nurse@example.test"
      placeholder="name@example.com"
      helperText="Use your work email."
      errorText="Email is required."
      [required]="true"
    />

    <np-text-input-control
      label="License number"
      controlId="license-field"
      value="RN-123"
      helperText="Provided by the nursing board."
      [readonly]="true"
    />

    <np-textarea-control
      label="Professional summary"
      controlId="summary-field"
      value="Experienced ICU nurse"
      placeholder="Brief summary"
      helperText="Keep this concise."
      [rows]="4"
    />

    <np-select-control
      label="Country"
      controlId="country-field"
      value="ae"
      helperText="Choose your current country."
      [required]="true"
      [options]="countryOptions"
    />

    <np-checkbox-control
      label="I confirm this information is accurate"
      controlId="confirm-field"
      helperText="Confirmation is required by the form using this control."
      [checked]="true"
      [required]="true"
    />

    <np-radio-group-control
      label="Preferred shift"
      controlId="shift-field"
      value="day"
      helperText="Select one option."
      [required]="true"
      [options]="shiftOptions"
    />

    <np-date-control
      label="Start date"
      controlId="start-date-field"
      value="2020-03-15"
      helperText="First day in this role."
      [required]="true"
      (valueChange)="lastDateValue = $event"
    />

    <np-date-control
      label="End date"
      controlId="end-date-field"
      value=""
      helperText="Last day in this role."
      errorText="End date must be on or after the start date."
      [disabled]="true"
    />
  `,
})
class FormControlsHost {
  lastDateValue = '';
  readonly countryOptions: readonly NpSelectOption[] = [
    { value: 'ae', label: 'United Arab Emirates' },
    { value: 'sa', label: 'Saudi Arabia' },
  ];

  readonly shiftOptions: readonly NpSelectOption[] = [
    { value: 'day', label: 'Day shift' },
    { value: 'night', label: 'Night shift' },
  ];
}

describe('standard form controls', () => {
  let fixture: ComponentFixture<FormControlsHost>;

  beforeEach(async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({ imports: [FormControlsHost] }).compileComponents();
    fixture = TestBed.createComponent(FormControlsHost);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('keeps visible labels persistent and placeholders supplementary for text inputs and textareas', () => {
    const email = query<HTMLInputElement>(fixture, '#email-field');
    const summary = query<HTMLTextAreaElement>(fixture, '#summary-field');
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Email address');
    expect(text).toContain('Professional summary');
    expect(email?.placeholder).toBe('name@example.com');
    expect(summary?.placeholder).toBe('Brief summary');
    expect(email?.getAttribute('aria-label')).toBeNull();
    expect(summary?.getAttribute('aria-label')).toBeNull();
  });

  it('renders persistent labels above native text, textarea, and date controls', () => {
    for (const id of ['email-field', 'license-field', 'summary-field', 'start-date-field']) {
      const control = query<HTMLElement>(fixture, `#${id}`);
      const label = query<HTMLLabelElement>(fixture, `label[for="${id}"]`);
      expect(control).not.toBeNull();
      expect(label).not.toBeNull();
      expect(label?.parentElement).toBe(control?.parentElement);
      expect(control?.closest('mat-form-field')).toBeNull();
    }
  });

  it('associates helper and structural error text without implementing validation behavior', () => {
    const email = query<HTMLInputElement>(fixture, '#email-field');
    const helper = query<HTMLElement>(fixture, '#email-field-helper');
    const error = query<HTMLElement>(fixture, '#email-field-error');
    const source = readText(`${formControlsDir}/standard-form-controls.ts`).toLowerCase();

    expect(helper?.textContent).toContain('Use your work email.');
    expect(error?.textContent).toContain('Email is required.');
    expect(email?.getAttribute('aria-describedby')).toContain('email-field-helper');
    expect(email?.getAttribute('aria-describedby')).toContain('email-field-error');
    expect(source).not.toContain('validator');
    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('problem');
  });

  it('preserves required optional disabled and readonly semantics distinctly', async () => {
    const email = query<HTMLInputElement>(fixture, '#email-field');
    const license = query<HTMLInputElement>(fixture, '#license-field');

    expect(email?.required).toBe(true);
    expect(license?.required).toBe(false);
    expect(license?.readOnly).toBe(true);
    expect(license?.disabled).toBe(false);
    expect((fixture.nativeElement.textContent as string).toLowerCase()).toContain('optional');
  });

  it('renders passed values for text input textarea select checkbox and radio controls', () => {
    expect(query<HTMLInputElement>(fixture, '#email-field')?.value).toBe('nurse@example.test');
    expect(query<HTMLInputElement>(fixture, '#license-field')?.value).toBe('RN-123');
    expect(query<HTMLTextAreaElement>(fixture, '#summary-field')?.value).toBe('Experienced ICU nurse');
    expect(fixture.debugElement.query(By.directive(MatSelect)).componentInstance.value).toBe('ae');
    expect(query<HTMLElement>(fixture, 'mat-checkbox')?.textContent).toContain('I confirm this information is accurate');
    expect(query<HTMLElement>(fixture, 'mat-radio-group')?.textContent).toContain('Day shift');
    expect(query<HTMLElement>(fixture, 'mat-radio-group')?.textContent).toContain('Night shift');
  });

  it('uses native labeled text/date inputs and preserves Material selection semantics', () => {
    expect(query(fixture, 'mat-form-field')).not.toBeNull();
    expect(query(fixture, 'input[matInput]')).toBeNull();
    expect(query(fixture, 'textarea[matInput]')).toBeNull();
    expect(query(fixture, 'label[for="email-field"]')).not.toBeNull();
    expect(query(fixture, 'label[for="summary-field"]')).not.toBeNull();
    expect(query(fixture, 'mat-select')).not.toBeNull();
    expect(query(fixture, 'mat-form-field mat-label')?.textContent).toContain('Country');
    expect(query(fixture, 'mat-checkbox')).not.toBeNull();
    expect(query(fixture, 'mat-radio-group')).not.toBeNull();
    expect(query(fixture, 'mat-radio-button')).not.toBeNull();
    expect(query<HTMLElement>(fixture, '[role="radiogroup"]')?.getAttribute('aria-describedby')).toContain(
      'shift-field-helper',
    );
  });

  it('keeps selection controls keyboard focusable through Material primitives', () => {
    const checkboxInput = query<HTMLInputElement>(fixture, 'mat-checkbox input');
    const radioInputs = all<HTMLInputElement>(fixture, 'mat-radio-button input');

    expect(checkboxInput?.type).toBe('checkbox');
    expect(checkboxInput?.disabled).toBe(false);
    expect(radioInputs.map((input) => input.type)).toEqual(['radio', 'radio']);
    expect(radioInputs.every((input) => input.name === 'shift-field')).toBe(true);
  });

  it('uses logical layout and approved form dimensions without parallel brand colors', () => {
    const styles = readText(`${formControlsDir}/standard-form-controls.scss`);

    expect(styles).toContain('min-block-size: var(--np-control-height)');
    expect(styles).toContain('border-radius: var(--np-control-radius)');
    expect(styles).toContain('padding-inline: var(--np-control-padding-inline)');
    expect(styles).toContain('min-inline-size: var(--np-control-height)');
    expect(styles).toContain('min-block-size: var(--np-space-48)');
    expect(styles).toContain('text-align: start');
    expect(styles).not.toMatch(/#[0-9A-Fa-f]{3,8}\b/);
    expect(styles).not.toContain('margin-left');
    expect(styles).not.toContain('margin-right');
  });

  it('does not pull forward advanced controls feature forms routing or T-FE-034 validation policy', () => {
    const source = readText(`${formControlsDir}/standard-form-controls.ts`).toLowerCase();
    const template = readText(`${formControlsDir}/standard-form-controls.html`).toLowerCase();
    const combined = `${source}\n${template}`;

    expect(combined).not.toContain('mat-slide-toggle');
    expect(combined).not.toContain('mat-autocomplete');
    expect(combined).not.toContain('mat-datepicker');
    expect(combined).not.toContain('file');
    expect(combined).not.toContain('router');
    expect(combined).not.toContain('auth');
    expect(combined).not.toContain('login');
    expect(combined).not.toContain('register');
    expect(combined).not.toContain('ngmodel');
    expect(combined).not.toContain('formcontrol');
  });

    it('renders native date inputs with the YYYY-MM-DD value contract', () => {
    const start = query<HTMLInputElement>(fixture, '#start-date-field');

    expect(start?.type).toBe('date');
    expect(start?.value).toBe('2020-03-15');
    expect(start?.required).toBe(true);
    expect(start?.getAttribute('aria-describedby')).toContain('start-date-field-helper');
    expect(fixture.nativeElement.textContent).toContain('First day in this role.');
  });

  it('surfaces date errors through the shared hint pattern and supports disabled state', () => {
    const end = query<HTMLInputElement>(fixture, '#end-date-field');

    expect(end?.type).toBe('date');
    expect(end?.disabled).toBe(true);
    expect(end?.getAttribute('aria-describedby')).toContain('end-date-field-error');
    const error = query<HTMLElement>(fixture, '#end-date-field-error');
    expect(error?.textContent).toContain('End date must be on or after the start date.');
  });

  it('emits YYYY-MM-DD strings when the native date input changes', () => {
    const start = query<HTMLInputElement>(fixture, '#start-date-field');
    expect(start).not.toBeNull();

    start?.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();

    expect((fixture.componentInstance as FormControlsHost).lastDateValue).toBe('2020-03-15');
  });

  it('keeps production components on external templates and styles only', () => {

    const source = readText(`${formControlsDir}/standard-form-controls.ts`);
    const template = readText(`${formControlsDir}/standard-form-controls.html`);

    expect(source).toContain("templateUrl: './standard-form-controls.html'");
    expect(source).toContain("styleUrl: './standard-form-controls.scss'");
    expect(source).not.toMatch(/template\s*:/);
    expect(source).not.toMatch(/styles\s*:/);
    expect(template).not.toContain('<style');
  });
});
