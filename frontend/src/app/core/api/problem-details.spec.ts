import type {
  CodedProblemDetails,
  ProblemDetails,
  RetryableProblemDetails,
  ValidationProblemDetails,
} from './generated/models';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { normalizeProblemDetails } from './problem-details';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

function genericPayload(): ProblemDetails {
  return {
    type: 'https://httpstatuses.com/404',
    title: 'Resource not found',
    status: 404,
    detail: 'Exam was not found.',
    traceId: 'trace-generic-1',
  };
}

function validationPayload(): ValidationProblemDetails {
  return {
    type: 'https://httpstatuses.com/400',
    title: 'Validation failed',
    status: 400,
    detail: 'One or more validation errors occurred.',
    traceId: 'trace-validation-1',
    errors: {
      Email: ['Email is required.'],
      Password: ['Password is required.', 'Password must be at least 8 characters.'],
    },
  };
}

function codedPayload(): CodedProblemDetails {
  return {
    type: 'https://httpstatuses.com/409',
    title: 'Conflict',
    status: 409,
    detail: 'Package attempt was already consumed.',
    traceId: 'trace-coded-1',
    code: 'package-attempt-consumed',
  };
}

function retryablePayload(): RetryableProblemDetails {
  return {
    type: 'https://httpstatuses.com/409',
    title: 'Conflict',
    status: 409,
    detail: 'Checkout initialization is already in progress.',
    traceId: 'trace-retryable-1',
    retryAfterSeconds: 12,
  };
}

describe('normalizeProblemDetails', () => {
  it('maps a generic ProblemDetails payload without inventing extensions', () => {
    const result = normalizeProblemDetails(genericPayload());

    expect(result.kind).toBe('generic');
    expect(result.type).toBe('https://httpstatuses.com/404');
    expect(result.title).toBe('Resource not found');
    expect(result.status).toBe(404);
    expect(result.detail).toBe('Exam was not found.');
    expect(result.traceId).toBe('trace-generic-1');
    expect(result.code).toBeUndefined();
    expect(result.errors).toBeUndefined();
    expect(result.retryAfterSeconds).toBeUndefined();
  });

  it('maps a ValidationProblemDetails payload with copied field errors', () => {
    const result = normalizeProblemDetails(validationPayload());

    expect(result.kind).toBe('validation');
    expect(result.status).toBe(400);
    expect(result.title).toBe('Validation failed');
    expect(result.detail).toBe('One or more validation errors occurred.');
    expect(result.traceId).toBe('trace-validation-1');
    expect(result.errors).toEqual({
      Email: ['Email is required.'],
      Password: ['Password is required.', 'Password must be at least 8 characters.'],
    });
    expect(result.code).toBeUndefined();
    expect(result.retryAfterSeconds).toBeUndefined();
  });

  it('maps a CodedProblemDetails payload with the backend code preserved', () => {
    const result = normalizeProblemDetails(codedPayload());

    expect(result.kind).toBe('coded');
    expect(result.status).toBe(409);
    expect(result.code).toBe('package-attempt-consumed');
    expect(result.detail).toBe('Package attempt was already consumed.');
    expect(result.traceId).toBe('trace-coded-1');
    expect(result.errors).toBeUndefined();
    expect(result.retryAfterSeconds).toBeUndefined();
  });

  it('maps a RetryableProblemDetails payload with retry metadata preserved', () => {
    const result = normalizeProblemDetails(retryablePayload());

    expect(result.kind).toBe('retryable');
    expect(result.status).toBe(409);
    expect(result.retryAfterSeconds).toBe(12);
    expect(result.detail).toBe('Checkout initialization is already in progress.');
    expect(result.traceId).toBe('trace-retryable-1');
    expect(result.code).toBeUndefined();
    expect(result.errors).toBeUndefined();
  });

  it('degrades nullish and empty payloads to a safe generic result', () => {
    for (const input of [null, undefined, {}]) {
      const result = normalizeProblemDetails(input);

      expect(result.kind).toBe('generic');
      expect(result.status).toBe(0);
      expect(result.type).toBe('');
      expect(result.title).toBe('');
      expect(result.detail).toBe('');
      expect(result.traceId).toBe('');
      expect(result.code).toBeUndefined();
      expect(result.errors).toBeUndefined();
      expect(result.retryAfterSeconds).toBeUndefined();
    }
  });

  it('preserves partial payloads without throwing', () => {
    const result = normalizeProblemDetails({ status: 404, detail: 'Exam was not found.' });

    expect(result.kind).toBe('generic');
    expect(result.status).toBe(404);
    expect(result.detail).toBe('Exam was not found.');
    expect(result.title).toBe('');
    expect(result.traceId).toBe('');
  });

  it('tolerates unknown extensions and malformed error shapes safely', () => {
    const result = normalizeProblemDetails({
      type: 'https://httpstatuses.com/400',
      title: 'Validation failed',
      status: 400,
      detail: 'Bad payload.',
      traceId: 'trace-unknown-1',
      errors: 'not-a-record',
      unknownExtension: { nested: true },
    });

    expect(result.kind).toBe('generic');
    expect(result.status).toBe(400);
    expect(result.errors).toBeUndefined();
  });

  it('keeps only string messages when validation error entries are mixed', () => {
    const result = normalizeProblemDetails({
      type: 'https://httpstatuses.com/400',
      title: 'Validation failed',
      status: 400,
      detail: 'Mixed errors.',
      traceId: 'trace-mixed-1',
      errors: {
        Email: ['Email is required.', 42, null],
        Skipped: 'not-an-array',
      },
    });

    expect(result.kind).toBe('validation');
    expect(result.errors).toEqual({ Email: ['Email is required.'] });
  });

  it('ignores empty codes and negative retry metadata instead of inventing categories', () => {
    const emptyCode = normalizeProblemDetails({
      type: 'https://httpstatuses.com/409',
      title: 'Conflict',
      status: 409,
      detail: 'Conflict without a code.',
      traceId: 'trace-empty-code-1',
      code: '',
    });

    expect(emptyCode.kind).toBe('generic');
    expect(emptyCode.code).toBeUndefined();

    const negativeRetry = normalizeProblemDetails({
      type: 'https://httpstatuses.com/409',
      title: 'Conflict',
      status: 409,
      detail: 'Conflict with invalid retry metadata.',
      traceId: 'trace-negative-retry-1',
      retryAfterSeconds: -5,
    });

    expect(negativeRetry.kind).toBe('generic');
    expect(negativeRetry.retryAfterSeconds).toBeUndefined();
  });

  it('resolves combined extensions deterministically with validation first', () => {
    const result = normalizeProblemDetails({
      type: 'https://httpstatuses.com/409',
      title: 'Conflict',
      status: 409,
      detail: 'Combined extensions.',
      traceId: 'trace-combined-1',
      errors: { Email: ['Email is required.'] },
      code: 'package-attempt-consumed',
      retryAfterSeconds: 12,
    });

    expect(result.kind).toBe('validation');
    expect(result.errors).toEqual({ Email: ['Email is required.'] });
    expect(result.code).toBeUndefined();
    expect(result.retryAfterSeconds).toBeUndefined();
  });

  it('preserves a string instance only when the payload carries one', () => {
    const withInstance = normalizeProblemDetails({
      ...genericPayload(),
      instance: '/api/v1/exams/missing',
    });

    expect(withInstance.instance).toBe('/api/v1/exams/missing');
    expect(normalizeProblemDetails(genericPayload()).instance).toBeUndefined();
  });

  it('does not mutate the source payload and returns a frozen result', () => {
    const payload = validationPayload();
    const snapshot = JSON.stringify(payload);

    const result = normalizeProblemDetails(payload);

    expect(JSON.stringify(payload)).toBe(snapshot);
    expect(Object.isFrozen(result)).toBe(true);
    expect(Object.isFrozen(result.errors)).toBe(true);
    expect(Object.isFrozen(result.errors?.['Email'])).toBe(true);
  });

  it('keeps the mapper free of UI, routing, auth, and retry orchestration behavior', () => {
    const source = readTextFile('src/app/core/api/problem-details.ts').toLowerCase();

    expect(source).not.toContain('toast');
    expect(source).not.toContain('snackbar');
    expect(source).not.toContain('router');
    expect(source).not.toContain('navigate');
    expect(source).not.toContain('logout');
    expect(source).not.toContain('settimeout');
    expect(source).not.toContain('setinterval');
    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('fetch(');
  });
});
