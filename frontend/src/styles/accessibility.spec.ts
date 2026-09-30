// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const stylesDir = join(nodeGlobal.process.cwd(), 'src', 'styles');
const accessibilityPath = join(stylesDir, '_accessibility.scss');
const mixinsPath = join(stylesDir, 'abstracts', '_mixins.scss');
const rootStylesPath = join(nodeGlobal.process.cwd(), 'src', 'styles.scss');

function readScss(path: string): string {
  return readFileSync(path, 'utf8');
}

describe('accessibility styles', () => {
  it('provides u-visually-hidden without display or visibility suppression', () => {
    const css = readScss(accessibilityPath);

    expect(css).toContain('.u-visually-hidden');
    expect(css).not.toMatch(/display\s*:\s*none/);
    expect(css).not.toMatch(/visibility\s*:\s*hidden/);
    expect(css).toMatch(/clip-path/);
    expect(css).toMatch(/overflow\s*:\s*hidden/);
  });

  it('uses approved focus-ring tokens for keyboard focus with an outline ring', () => {
    const css = readScss(accessibilityPath);

    expect(css).toMatch(
      /:focus-visible\s*\{[^}]*outline\s*:\s*var\(--np-focus-ring-width\)\s+solid\s+var\(--np-focus-ring-color\)\s*;[^}]*outline-offset\s*:\s*var\(--np-focus-ring-offset\)\s*;[^}]*\}/,
    );
    expect(css).not.toContain('0 0 0 4 #006B66');
  });

  it('provides touch-target default and mobile minimum sizing with logical properties', () => {
    const mixins = readScss(mixinsPath);

    expect(mixins).toMatch(
      /@mixin\s+touch-target\(\$mobile:\s*false\)\s*\{\s*@if\s+\$mobile\s*\{\s*min-inline-size\s*:\s*48px\s*;\s*min-block-size\s*:\s*48px\s*;\s*\}\s*@else\s*\{\s*min-inline-size\s*:\s*44px\s*;\s*min-block-size\s*:\s*44px\s*;\s*\}\s*\}/,
    );
  });

  it('wires the accessibility partial without physical-direction CSS or global focus removal', () => {
    const root = readScss(rootStylesPath);
    const accessibility = readScss(accessibilityPath);
    const mixins = readScss(mixinsPath);
    const combined = `${accessibility}\n${mixins}`;

    expect(root).toContain('styles/tokens');
    expect(root).toContain('styles/accessibility');
    expect(accessibility).not.toContain('0 0 0 4 #006B66');

    for (const banned of ['margin-left', 'margin-right', 'padding-left', 'padding-right']) {
      expect(combined).not.toContain(banned);
    }

    expect(combined).not.toMatch(/^\s*left\s*:/m);
    expect(combined).not.toMatch(/^\s*right\s*:/m);
    expect(combined).not.toMatch(/float\s*:\s*(left|right)/);
    expect(combined).not.toMatch(/text-align\s*:\s*(left|right)/);
    expect(combined).not.toMatch(/\*\s*:\s*focus/);
  });
});
