import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';

const FIELD_NAMES = Object.freeze([
  'CountryId', 'ExamCategoryId', 'Title', 'Slug',
  'Description', 'Instructions', 'DurationMinutes',
  'PassingScorePercentage', 'IsFree',
] as const);

export function safeAdminExamValidation(
  error: unknown,
  labels: Record<string, string>,
  review: (fieldLabel: string) => string,
): NormalizedProblemDetails | undefined {
  const source = typeof error === 'object' && error !== null && 'error' in error
    ? (error as { error?: unknown }).error : error;
  const normalized = normalizeProblemDetails(source);
  if (normalized.kind !== 'validation') return undefined;

  const errors: Record<string, readonly string[]> = {};
  for (const field of FIELD_NAMES) {
    if (normalized.errors?.[field]?.length || normalized.errors?.[`Request.${field}`]?.length) {
      errors[field] = [review(labels[field] ?? field)];
    }
  }
  return { ...normalized, title: '', detail: '', errors };
}
