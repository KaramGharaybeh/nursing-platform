import { normalizeProblemDetails } from '../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../core/api/problem-details';

const FIELD_LABELS = Object.freeze({
  CountryId: 'country', ExamCategoryId: 'exam category', Title: 'title', Slug: 'slug',
  Description: 'description', Instructions: 'instructions', DurationMinutes: 'duration',
  PassingScorePercentage: 'passing score', IsFree: 'price type',
});

export function safeAdminExamValidation(error: unknown): NormalizedProblemDetails | undefined {
  const source = typeof error === 'object' && error !== null && 'error' in error
    ? (error as { error?: unknown }).error : error;
  const normalized = normalizeProblemDetails(source);
  if (normalized.kind !== 'validation') return undefined;

  const errors: Record<string, readonly string[]> = {};
  for (const [field, label] of Object.entries(FIELD_LABELS)) {
    if (normalized.errors?.[field]?.length || normalized.errors?.[`Request.${field}`]?.length) {
      errors[field] = [`Review ${label}.`];
    }
  }
  return { ...normalized, title: '', detail: '', errors };
}
