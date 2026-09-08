import type {
  CodedProblemDetails,
  ProblemDetails,
  RetryableProblemDetails,
  ValidationProblemDetails,
} from './generated/models';

export type NormalizedProblemDetailsKind = 'generic' | 'validation' | 'coded' | 'retryable';

export interface NormalizedProblemDetails {
  readonly kind: NormalizedProblemDetailsKind;
  readonly type: string;
  readonly title: string;
  readonly status: number;
  readonly detail: string;
  readonly traceId: string;
  readonly instance?: string;
  readonly code?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
  readonly retryAfterSeconds?: number;
}

export type ProblemDetailsSource =
  | ProblemDetails
  | ValidationProblemDetails
  | CodedProblemDetails
  | RetryableProblemDetails;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function readString(record: Record<string, unknown>, key: string): string {
  const value: unknown = record[key];
  return typeof value === 'string' ? value : '';
}

function readStatus(record: Record<string, unknown>): number {
  const value: unknown = record['status'];
  return typeof value === 'number' && Number.isInteger(value) && value >= 0 ? value : 0;
}

function readErrors(record: Record<string, unknown>): Record<string, string[]> | undefined {
  const value: unknown = record['errors'];
  if (!isRecord(value)) {
    return undefined;
  }
  const errors: Record<string, string[]> = {};
  for (const key of Object.keys(value)) {
    const messages: unknown = value[key];
    if (!Array.isArray(messages)) {
      continue;
    }
    errors[key] = messages.filter(
      (message): message is string => typeof message === 'string',
    );
  }
  return errors;
}

function readCode(record: Record<string, unknown>): string | undefined {
  const value: unknown = record['code'];
  return typeof value === 'string' && value !== '' ? value : undefined;
}

function readRetryAfterSeconds(record: Record<string, unknown>): number | undefined {
  const value: unknown = record['retryAfterSeconds'];
  if (typeof value !== 'number' || !Number.isFinite(value) || value < 0) {
    return undefined;
  }
  return Math.floor(value);
}

function readInstance(record: Record<string, unknown>): string | undefined {
  const value: unknown = record['instance'];
  return typeof value === 'string' && value !== '' ? value : undefined;
}

export function normalizeProblemDetails(input: unknown): NormalizedProblemDetails {
  const record: Record<string, unknown> = isRecord(input) ? input : {};
  const errors = readErrors(record);
  const code = errors === undefined ? readCode(record) : undefined;
  const retryAfterSeconds = errors === undefined && code === undefined
    ? readRetryAfterSeconds(record)
    : undefined;

  const kind: NormalizedProblemDetailsKind =
    errors !== undefined ? 'validation'
    : code !== undefined ? 'coded'
    : retryAfterSeconds !== undefined ? 'retryable'
    : 'generic';

  const frozenErrors =
    errors === undefined
      ? undefined
      : Object.freeze(
          Object.fromEntries(
            Object.entries(errors).map(
              ([key, messages]): [string, readonly string[]] => [key, Object.freeze(messages)],
            ),
          ),
        );

  const instance = readInstance(record);

  return Object.freeze({
    kind,
    type: readString(record, 'type'),
    title: readString(record, 'title'),
    status: readStatus(record),
    detail: readString(record, 'detail'),
    traceId: readString(record, 'traceId'),
    ...(instance === undefined ? {} : { instance }),
    ...(code === undefined ? {} : { code }),
    ...(frozenErrors === undefined ? {} : { errors: frozenErrors }),
    ...(retryAfterSeconds === undefined ? {} : { retryAfterSeconds }),
  });
}
