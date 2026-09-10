// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { RETURN_URL_QUERY_KEY, isSafeReturnUrl } from './safe-return';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

describe('safe-return', () => {
  it('uses the exact returnUrl query key', () => {
    expect(RETURN_URL_QUERY_KEY).toBe('returnUrl');
  });

  it('accepts a normal internal absolute path beginning with exactly one slash', () => {
    expect(isSafeReturnUrl('/account')).toBe(true);
    expect(isSafeReturnUrl('/nurse/profile')).toBe(true);
    expect(isSafeReturnUrl('/employer/requests/request-1')).toBe(true);
    expect(isSafeReturnUrl('/exams/exam-1/sessions/session-2/result')).toBe(true);
  });

  it('accepts the root path', () => {
    expect(isSafeReturnUrl('/')).toBe(true);
  });

  it('accepts internal paths with an optional query string', () => {
    expect(isSafeReturnUrl('/nurse/profile?tab=experience')).toBe(true);
    expect(isSafeReturnUrl('/employer/requests?page=2&size=10')).toBe(true);
  });

  it('accepts internal paths with an optional fragment', () => {
    expect(isSafeReturnUrl('/exams/exam-1#overview')).toBe(true);
    expect(isSafeReturnUrl('/nurse/profile?tab=experience#skills')).toBe(true);
  });

  it('rejects empty and non-path external-style values', () => {
    expect(isSafeReturnUrl('')).toBe(false);
    expect(isSafeReturnUrl('   ')).toBe(false);
    expect(isSafeReturnUrl('account')).toBe(false);
    expect(isSafeReturnUrl('?query=1')).toBe(false);
    expect(isSafeReturnUrl('#fragment')).toBe(false);
    expect(isSafeReturnUrl(' /account')).toBe(false);
  });

  it('rejects protocol-relative values beginning with //', () => {
    expect(isSafeReturnUrl('//example.com/path')).toBe(false);
    expect(isSafeReturnUrl('//')).toBe(false);
  });

  it('rejects absolute external URLs', () => {
    expect(isSafeReturnUrl('http://example.com/path')).toBe(false);
    expect(isSafeReturnUrl('https://example.com/path')).toBe(false);
    expect(isSafeReturnUrl('http:/example.com')).toBe(false);
  });

  it('rejects scheme-bearing input such as javascript:, data:, mailto:, and ftp:', () => {
    expect(isSafeReturnUrl('javascript:alert(1)')).toBe(false);
    expect(isSafeReturnUrl('data:text/html,<script>alert(1)</script>')).toBe(false);
    expect(isSafeReturnUrl('mailto:user@example.com')).toBe(false);
    expect(isSafeReturnUrl('ftp://example.com/file')).toBe(false);
    expect(isSafeReturnUrl('/javascript:alert(1)')).toBe(false);
    expect(isSafeReturnUrl('/https://example.com/path')).toBe(false);
  });

  it('rejects backslash-based path confusion', () => {
    expect(isSafeReturnUrl('\\example.com\\path')).toBe(false);
    expect(isSafeReturnUrl('/\\example.com')).toBe(false);
    expect(isSafeReturnUrl('/path\\evil')).toBe(false);
  });

  it('rejects control-character input', () => {
    expect(isSafeReturnUrl('/path\n')).toBe(false);
    expect(isSafeReturnUrl('/path\t')).toBe(false);
    expect(isSafeReturnUrl('/path\r')).toBe(false);
    expect(isSafeReturnUrl('/pa\u0000th')).toBe(false);
  });

  it('rejects encoded path confusion while preserving encoded query values', () => {
    expect(isSafeReturnUrl('/%2Fexample.com/path')).toBe(false);
    expect(isSafeReturnUrl('/%5Cexample.com/path')).toBe(false);
    expect(isSafeReturnUrl('/%00path')).toBe(false);
    expect(isSafeReturnUrl('/%6A%61%76%61%73%63%72%69%70%74%3Aalert(1)')).toBe(false);
    expect(isSafeReturnUrl('/search?q=https%3A%2F%2Fexample.com')).toBe(true);
  });

  it('keeps the safe-return helper pure and free of Angular and navigation behavior', () => {
    const source = readTextFile('src/app/core/routing/safe-return.ts');
    const lowered = source.toLowerCase();

    expect(lowered).not.toContain('@angular');
    expect(lowered).not.toContain('router');
    expect(lowered).not.toContain('guard');
    expect(lowered).not.toContain('navigate');
    expect(lowered).not.toContain('permission');
    expect(lowered).not.toContain('role');
  });
});
