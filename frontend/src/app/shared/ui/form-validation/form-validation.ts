import type { NormalizedProblemDetails } from '../../../core/api/problem-details';

export interface NpValidationSummaryItem {
  readonly fieldKey: string;
  readonly controlId: string;
  readonly label: string;
  readonly message: string;
}

export interface NpFormValidationOptions {
  readonly fieldLabels?: Readonly<Record<string, string>>;
  readonly controlIds?: Readonly<Record<string, string>>;
  readonly summaryTitle?: string;
  readonly formErrorFallback?: string;
}

export interface NpValidationSummary {
  readonly hasErrors: boolean;
  readonly title: string;
  readonly items: readonly NpValidationSummaryItem[];
  readonly formErrors: readonly string[];
}

const DEFAULT_SUMMARY_TITLE = 'Check the highlighted fields';
const DEFAULT_FORM_ERROR = 'The form could not be submitted.';
const GENERIC_FIELD_LABEL = 'This field';

function cleanText(value: unknown): string | undefined {
  if (typeof value !== 'string') {
    return undefined;
  }
  const trimmed = value.trim().replace(/\s+/g, ' ');
  return trimmed === '' ? undefined : trimmed;
}

function sanitizeDerivedLabel(fieldKey: string): string {
  const withoutMarkup = fieldKey.replace(/[<>"'`&]/g, '');
  const spaced = withoutMarkup.replace(/[_-]+/g, ' ').replace(/\s+/g, ' ').trim();
  return spaced === '' ? GENERIC_FIELD_LABEL : spaced;
}

function sanitizeControlId(value: string): string | undefined {
  const trimmed = value.trim();
  if (trimmed === '') {
    return undefined;
  }
  const slug = trimmed.replace(/\s+/g, '-').replace(/[^A-Za-z0-9\-_:.]/g, '');
  return slug === '' ? undefined : slug;
}

function isErrorRecord(value: unknown): value is Readonly<Record<string, readonly unknown[]>> {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    return false;
  }
  return true;
}

export function toFieldErrorText(messages: readonly unknown[] | undefined): string {
  if (messages === undefined) {
    return '';
  }
  const parts: string[] = [];
  for (const message of messages) {
    const cleaned = cleanText(message);
    if (cleaned !== undefined) {
      parts.push(cleaned);
    }
  }
  return parts.join(' ');
}

export function toFormValidationItems(
  errors: Readonly<Record<string, readonly unknown[]>> | undefined | null,
  options: NpFormValidationOptions = {},
): readonly NpValidationSummaryItem[] {
  if (!isErrorRecord(errors)) {
    return Object.freeze([]);
  }
  const keys = Object.keys(errors).sort();
  const items: NpValidationSummaryItem[] = [];
  for (const fieldKey of keys) {
    const message = toFieldErrorText(errors[fieldKey]);
    if (message === '') {
      continue;
    }
    const configuredLabel = cleanText(options.fieldLabels?.[fieldKey]);
    const label = configuredLabel ?? sanitizeDerivedLabel(fieldKey);
    const configuredControlId =
      options.controlIds === undefined ? undefined : sanitizeControlId(options.controlIds[fieldKey] ?? '');
    const controlId = configuredControlId ?? sanitizeControlId(fieldKey) ?? `field-${items.length}`;
    items.push(Object.freeze({ fieldKey, controlId, label, message }));
  }
  return Object.freeze(items);
}

export function toFormValidationSummary(
  normalized: NormalizedProblemDetails | null | undefined,
  options: NpFormValidationOptions = {},
): NpValidationSummary {
  const title = cleanText(options.summaryTitle) ?? DEFAULT_SUMMARY_TITLE;
  const formErrorFallback = cleanText(options.formErrorFallback) ?? DEFAULT_FORM_ERROR;
  if (normalized === null || normalized === undefined) {
    return Object.freeze({ hasErrors: false, title, items: Object.freeze([]), formErrors: Object.freeze([]) });
  }
  if (normalized.kind === 'validation') {
    const items = toFormValidationItems(normalized.errors, options);
    if (items.length > 0) {
      return Object.freeze({ hasErrors: true, title, items, formErrors: Object.freeze([]) });
    }
    return Object.freeze({
      hasErrors: true,
      title,
      items,
      formErrors: Object.freeze([formErrorFallback]),
    });
  }
  return Object.freeze({ hasErrors: true, title, items: Object.freeze([]), formErrors: Object.freeze([formErrorFallback]) });
}
