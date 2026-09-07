import { TestBed } from '@angular/core/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import {
  DEFAULT_DIRECTION,
  DEFAULT_LOCALE,
  LOCALE_DIRECTION,
  LOCALE_FONT_STACK,
  LocaleDirectionService,
} from './locale-direction.service';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const servicePath = join(
  nodeGlobal.process.cwd(),
  'src',
  'app',
  'core',
  'locale',
  'locale-direction.service.ts',
);

function readServiceSource(): string {
  return readFileSync(servicePath, 'utf8');
}

describe('LocaleDirectionService', () => {
  const originalLang = document.documentElement.lang;
  const originalDir = document.documentElement.dir;

  afterEach(() => {
    document.documentElement.lang = originalLang;
    document.documentElement.dir = originalDir;
  });

  it('uses en as the default locale', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    expect(service.locale()).toBe('en');
    expect(DEFAULT_LOCALE).toBe('en');
  });

  it('uses ltr as the default direction', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    expect(service.direction()).toBe('ltr');
    expect(DEFAULT_DIRECTION).toBe('ltr');
  });

  it('maps en to ltr', () => {
    expect(LOCALE_DIRECTION['en']).toBe('ltr');
  });

  it('maps ar to rtl', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    service.setLocale('ar');

    expect(service.locale()).toBe('ar');
    expect(service.direction()).toBe('rtl');
    expect(LOCALE_DIRECTION['ar']).toBe('rtl');
  });

  it('synchronizes document lang from central state', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    expect(document.documentElement.lang).toBe('en');

    service.setLocale('ar');

    expect(document.documentElement.lang).toBe('ar');
  });

  it('synchronizes document dir from central state', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    expect(document.documentElement.dir).toBe('ltr');

    service.setLocale('ar');

    expect(document.documentElement.dir).toBe('rtl');

    service.setLocale('en');

    expect(document.documentElement.dir).toBe('ltr');
  });

  it('maps both foundation locales to the approved temporary font stack', () => {
    expect(LOCALE_FONT_STACK['en']).toBe('system-ui, sans-serif');
    expect(LOCALE_FONT_STACK['ar']).toBe('system-ui, sans-serif');

    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocaleDirectionService);

    expect(service.fontStack()).toBe('system-ui, sans-serif');

    service.setLocale('ar');

    expect(service.fontStack()).toBe('system-ui, sans-serif');
  });

  it('introduces no persistence mechanism', () => {
    const source = readServiceSource();

    expect(source).not.toContain('localStorage');
    expect(source).not.toContain('sessionStorage');
    expect(source).not.toContain('document.cookie');
    expect(source).not.toContain('IndexedDB');
    expect(source).not.toContain('indexedDB');
  });

  it('introduces no CDK or Bidi dependency', () => {
    const source = readServiceSource();

    expect(source).not.toContain('@angular/cdk');
    expect(source).not.toContain('BidiModule');
    expect(source).not.toContain('Directionality');
    expect(source).not.toMatch(/\bBidi\b/);
  });

  it('introduces no physical-direction CSS', () => {
    const source = readServiceSource();

    expect(source).not.toContain('margin-left');
    expect(source).not.toContain('margin-right');
    expect(source).not.toContain('padding-left');
    expect(source).not.toContain('padding-right');
    expect(source).not.toMatch(/^\s*left\s*:/m);
    expect(source).not.toMatch(/^\s*right\s*:/m);
    expect(source).not.toMatch(/float\s*:\s*(left|right)/);
    expect(source).not.toMatch(/text-align\s*:\s*(left|right)/);
  });
});
