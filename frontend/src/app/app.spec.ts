import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { App } from './app';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const appDir = join(nodeGlobal.process.cwd(), 'src', 'app');

function readAppFile(name: string): string {
  return readFileSync(join(appDir, name), 'utf8');
}

describe('App shell frame', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('renders exactly one main landmark', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelectorAll('main').length).toBe(1);
  });

  it('exposes the main content target with stable id and focusability', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const main = compiled.querySelector('main');
    expect(main?.getAttribute('id')).toBe('main-content');
    expect(main?.getAttribute('tabindex')).toBe('-1');
  });

  it('renders the router outlet inside the main landmark', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const main = compiled.querySelector('main#main-content');
    expect(main).not.toBeNull();
    expect(main?.querySelector('router-outlet')).not.toBeNull();
  });

  it('removes Angular scaffold placeholder content and links', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.angular-logo')).toBeNull();
    expect(compiled.querySelector('.pill-group')).toBeNull();
    expect(compiled.querySelector('.social-links')).toBeNull();
    expect(compiled.textContent).not.toContain('Hello, nursing-platform-frontend');
    expect(compiled.textContent).not.toContain('Congratulations');
  });

  it('contains no navigation or product link list', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('nav')).toBeNull();
    expect(compiled.querySelectorAll('a').length).toBe(0);
  });

  it('keeps component separation with external template and style metadata', () => {
    const source = readAppFile('app.ts');
    expect(source).toMatch(/templateUrl\s*:\s*['"]\.\/app\.html['"]/);
    expect(source).toMatch(/styleUrl\s*:\s*['"]\.\/app\.scss['"]/);
    expect(source).not.toMatch(/template\s*:/);
    expect(source).not.toMatch(/styles\s*:/);
  });

  it('keeps the shell template free of embedded style blocks', () => {
    const template = readAppFile('app.html');
    expect(template.toLowerCase()).not.toContain('<style');
  });

  it('styles the shell with logical properties and approved tokens only', () => {
    const css = readAppFile('app.scss');
    expect(css).toMatch(/min-block-size/);
    expect(css).toMatch(/padding-inline\s*:\s*var\(\s*--np-page-gutter\s*\)/);
    expect(css).toMatch(/var\(\s*--np-color-brand-2\s*\)/);
    expect(css).not.toMatch(/#[0-9a-fA-F]{3,8}/);
    expect(css.toLowerCase()).not.toContain('oklch');
    for (const banned of ['margin-left', 'margin-right', 'padding-left', 'padding-right']) {
      expect(css).not.toContain(banned);
    }
    expect(css).not.toMatch(/^\s*left\s*:/m);
    expect(css).not.toMatch(/^\s*right\s*:/m);
    expect(css).not.toMatch(/float\s*:\s*(left|right)/);
    expect(css).not.toMatch(/text-align\s*:\s*(left|right)/);
  });
});
