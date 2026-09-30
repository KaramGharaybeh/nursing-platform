import { TestBed } from '@angular/core/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { ApiConfiguration } from './generated/api-configuration';
import { RequestBuilder } from './generated/request-builder';
import { listExams } from './generated/fn/nursing-platform-web-api/list-exams';
import {
  API_PATH_PREFIX,
  DEFAULT_API_ORIGIN,
  buildApiPath,
  normalizeApiOrigin,
  provideApiConfig,
  resolveApiRootUrl,
} from './api-config';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

describe('api-config', () => {
  it('exposes the exact API path prefix', () => {
    expect(API_PATH_PREFIX).toBe('/api/v1');
  });

  it('uses an empty string as the default API origin', () => {
    expect(DEFAULT_API_ORIGIN).toBe('');
  });

  it('resolves the default generated root URL to an empty string', () => {
    expect(resolveApiRootUrl()).toBe('');
    expect(resolveApiRootUrl({})).toBe('');
    expect(resolveApiRootUrl({ apiOrigin: '' })).toBe('');
  });

  it('proves the default request URL through generated operation path composition', () => {
    expect(listExams.PATH).toBe('/api/v1/exams');
    expect(`${resolveApiRootUrl()}${listExams.PATH}`).toBe('/api/v1/exams');
    expect(buildApiPath('/exams')).toBe('/api/v1/exams');
    expect(buildApiPath('exams')).toBe('/api/v1/exams');

    TestBed.configureTestingModule({ providers: [provideApiConfig()] });
    const config = TestBed.inject(ApiConfiguration);
    const request = new RequestBuilder(config.rootUrl, listExams.PATH, 'get').build();

    expect(config.rootUrl).toBe('');
    expect(request.url).toBe('/api/v1/exams');
  });

  it('normalizes a configured origin by removing trailing slashes', () => {
    expect(normalizeApiOrigin('http://localhost:5167/')).toBe('http://localhost:5167');
    expect(normalizeApiOrigin('http://localhost:5167///')).toBe('http://localhost:5167');
    expect(normalizeApiOrigin('http://localhost:5167')).toBe('http://localhost:5167');
    expect(normalizeApiOrigin(undefined)).toBe('');
    expect(normalizeApiOrigin('')).toBe('');
  });

  it('normalizes a configured origin to origin only without a double slash', () => {
    expect(resolveApiRootUrl({ apiOrigin: 'http://localhost:5167/' })).toBe(
      'http://localhost:5167',
    );
    expect(resolveApiRootUrl({ apiOrigin: 'http://localhost:5167' })).toBe(
      'http://localhost:5167',
    );
    expect(buildApiPath('/exams', { apiOrigin: 'http://localhost:5167/' })).toBe(
      'http://localhost:5167/api/v1/exams',
    );
  });

  it('proves the configured request URL through generated operation path composition', () => {
    TestBed.configureTestingModule({
      providers: [provideApiConfig({ apiOrigin: 'http://localhost:5167/' })],
    });

    const config = TestBed.inject(ApiConfiguration);
    const request = new RequestBuilder(config.rootUrl, listExams.PATH, 'get').build();

    expect(config.rootUrl).toBe('http://localhost:5167');
    expect(request.url).toBe('http://localhost:5167/api/v1/exams');
    expect(request.url).not.toContain('//api/v1');
  });

  it('supplies the generated ApiConfiguration with an empty default root URL', () => {
    TestBed.configureTestingModule({ providers: [provideApiConfig()] });

    const config = TestBed.inject(ApiConfiguration);

    expect(config.rootUrl).toBe('');
  });

  it('changes the generated root URL to origin only without generated edits', () => {
    TestBed.configureTestingModule({
      providers: [provideApiConfig({ apiOrigin: 'http://localhost:5167/' })],
    });

    const config = TestBed.inject(ApiConfiguration);

    expect(config.rootUrl).toBe('http://localhost:5167');

    const source = readTextFile('src/app/core/api/api-config.ts');
    expect(source).toContain('provideApiConfiguration');
    expect(source).toContain('./generated/api-configuration');
  });

  it('keeps the public config shape free of private fields', () => {
    const source = readTextFile('src/app/core/api/api-config.ts').toLowerCase();

    expect(source).not.toContain('password');
    expect(source).not.toContain('bearer');
    expect(source).not.toContain('secret');
    expect(source).not.toContain('credential');
    expect(source).not.toContain('authorization');
    expect(source).not.toContain('token');
  });

  it('maps exactly /api/v1 to the local backend and preserves the path', () => {
    const proxy = JSON.parse(readTextFile('proxy.conf.json')) as Record<string, { target?: string } & Record<string, unknown>>;

    expect(Object.keys(proxy)).toEqual(['/api/v1']);
    expect(proxy['/api/v1']['target']).toBe('http://localhost:5167');
    expect(proxy['/api/v1']['pathRewrite']).toBeUndefined();
  });
});
