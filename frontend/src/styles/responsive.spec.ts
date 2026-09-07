// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const stylesDir = join(nodeGlobal.process.cwd(), 'src', 'styles');
const abstractsResponsivePath = join(stylesDir, 'abstracts', '_responsive.scss');
const responsivePath = join(stylesDir, '_responsive.scss');
const rootStylesPath = join(nodeGlobal.process.cwd(), 'src', 'styles.scss');

function readScss(path: string): string {
  return readFileSync(path, 'utf8');
}

function extractBreakpointsBody(css: string): string {
  const match = css.match(/\$breakpoints\s*:\s*\(([\s\S]*?)\)\s*;/);

  expect(match, 'expected $breakpoints: (...) map declaration').not.toBeNull();

  return match?.[1] ?? '';
}

function parseBreakpointEntries(body: string): Record<string, string> {
  const entries: Record<string, string> = {};
  const parts = body
    .split(',')
    .map((part) => part.trim())
    .filter((part) => part.length > 0);

  for (const part of parts) {
    const entry = part.match(/^([a-zA-Z0-9_-]+)\s*:\s*([0-9]+px)$/);

    expect(entry, `expected breakpoint entry of the form name: <n>px, got: ${part}`).not.toBeNull();

    if (entry !== null) {
      entries[entry[1]] = entry[2];
    }
  }

  return entries;
}

function extractBalancedBody(css: string, openBraceIndex: number): string {
  let depth = 0;

  for (let index = openBraceIndex; index < css.length; index += 1) {
    if (css[index] === '{') {
      depth += 1;
    }

    if (css[index] === '}') {
      depth -= 1;

      if (depth === 0) {
        return css.slice(openBraceIndex + 1, index);
      }
    }
  }

  throw new Error('expected balanced braces but reached end of file');
}

function extractMixinBody(css: string, mixinName: string): string {
  const marker = css.match(new RegExp(`@mixin\\s+${mixinName}\\s*(\\([^)]*\\))?\\s*\\{`));

  expect(marker, `expected @mixin ${mixinName} {`).not.toBeNull();

  const openBraceIndex = (marker?.index ?? 0) + marker![0].lastIndexOf('{');

  return extractBalancedBody(css, openBraceIndex);
}

function extractIncludeBranch(css: string, breakpoint: string): string {
  const marker = css.match(new RegExp(`@include[^{]*respond-from\\(\\s*${breakpoint}\\s*\\)`));

  expect(marker, `expected @include ... respond-from(${breakpoint}) branch`).not.toBeNull();

  const openBraceIndex = css.indexOf('{', (marker?.index ?? 0) + marker![0].length);

  expect(openBraceIndex).toBeGreaterThan(-1);

  return extractBalancedBody(css, openBraceIndex);
}

function removeIncludeBranches(css: string): string {
  let output = '';
  let cursor = 0;

  while (cursor < css.length) {
    const nextInclude = css.indexOf('@include', cursor);

    if (nextInclude === -1) {
      output += css.slice(cursor);
      break;
    }

    const openBraceIndex = css.indexOf('{', nextInclude);

    if (openBraceIndex === -1) {
      output += css.slice(cursor);
      break;
    }

    output += css.slice(cursor, nextInclude);

    let depth = 0;
    let end = openBraceIndex;

    for (; end < css.length; end += 1) {
      if (css[end] === '{') {
        depth += 1;
      }

      if (css[end] === '}') {
        depth -= 1;

        if (depth === 0) {
          break;
        }
      }
    }

    cursor = end + 1;
  }

  return output;
}

describe('responsive foundation', () => {
  it('defines only the approved breakpoint map entries', () => {
    const css = readScss(abstractsResponsivePath);
    const body = extractBreakpointsBody(css);
    const entries = parseBreakpointEntries(body);

    expect(entries).toEqual({
      tablet: '600px',
      desktop: '960px',
      large: '1280px',
      wide: '1920px',
    });
    expect(Object.keys(entries)).toHaveLength(4);
    expect(body).not.toMatch(/mobile\s*:/);
  });

  it('implements respond-from from the map with min-width and unknown-breakpoint error', () => {
    const css = readScss(abstractsResponsivePath);
    const mixinBody = extractMixinBody(css, 'respond-from');

    expect(mixinBody).toMatch(/map\.get\s*\(\s*\$breakpoints\s*,\s*\$breakpoint\s*\)/);
    expect(mixinBody).toMatch(/@if\s+not\s+\$value/);
    expect(mixinBody).toMatch(/@error/);
    expect(mixinBody).toMatch(/\$breakpoint/);
    expect(mixinBody).toMatch(/@media\s*\(\s*min-width\s*:\s*\$value\s*\)/);
    expect(mixinBody).toMatch(/@content/);

    const mediaIndex = mixinBody.indexOf('@media');
    const contentIndex = mixinBody.indexOf('@content');

    expect(mediaIndex).toBeGreaterThan(-1);
    expect(contentIndex).toBeGreaterThan(mediaIndex);
  });

  it('implements page-gutter with logical padding only', () => {
    const css = readScss(abstractsResponsivePath);
    const mixinBody = extractMixinBody(css, 'page-gutter');

    expect(mixinBody).toMatch(/padding-inline\s*:\s*var\(\s*--np-page-gutter\s*\)\s*;/);
    expect(mixinBody).not.toContain('padding-left');
    expect(mixinBody).not.toContain('padding-right');
    expect(mixinBody).not.toContain('margin-left');
    expect(mixinBody).not.toContain('margin-right');
    expect(mixinBody).not.toMatch(/^\s*left\s*:/m);
    expect(mixinBody).not.toMatch(/^\s*right\s*:/m);
  });

  it('sets base and stepped page gutters in the correct branches', () => {
    const css = readScss(responsivePath);
    const baseCss = removeIncludeBranches(css);
    const tabletBranch = extractIncludeBranch(css, 'tablet');
    const desktopBranch = extractIncludeBranch(css, 'desktop');

    expect(baseCss).toMatch(/:root\s*\{[^}]*--np-page-gutter\s*:\s*16px\s*;[^}]*\}/);
    expect(tabletBranch).toMatch(/--np-page-gutter\s*:\s*24px/);
    expect(desktopBranch).toMatch(/--np-page-gutter\s*:\s*32px/);
    expect(css).not.toMatch(/respond-from\(\s*large\s*\)[^{]*\{[^}]*--np-page-gutter/s);
    expect(css).not.toMatch(/respond-from\(\s*wide\s*\)[^{]*\{[^}]*--np-page-gutter/s);
  });

  it('wires the responsive foundation while preserving tokens and accessibility', () => {
    const root = readScss(rootStylesPath);
    const tokensIndex = root.indexOf('styles/tokens');
    const accessibilityIndex = root.indexOf('styles/accessibility');
    const responsiveIndex = root.indexOf('styles/responsive');

    expect(tokensIndex).toBeGreaterThan(-1);
    expect(accessibilityIndex).toBeGreaterThan(-1);
    expect(responsiveIndex).toBeGreaterThan(-1);
    expect(tokensIndex).toBeLessThan(accessibilityIndex);
    expect(accessibilityIndex).toBeLessThan(responsiveIndex);
    expect(root).toMatch(/@use\s+['"]styles\/tokens['"]/);
    expect(root).toMatch(/@use\s+['"]styles\/accessibility['"]/);
    expect(root).toMatch(/@use\s+['"]styles\/responsive['"]/);
  });

  it('contains no physical-direction CSS in new responsive sources', () => {
    const abstracts = readScss(abstractsResponsivePath);
    const responsive = readScss(responsivePath);
    const combined = `${abstracts}\n${responsive}`;

    for (const banned of ['margin-left', 'margin-right', 'padding-left', 'padding-right']) {
      expect(combined).not.toContain(banned);
    }

    expect(combined).not.toMatch(/^\s*left\s*:/m);
    expect(combined).not.toMatch(/^\s*right\s*:/m);
    expect(combined).not.toMatch(/float\s*:\s*(left|right)/);
    expect(combined).not.toMatch(/text-align\s*:\s*(left|right)/);
  });
});
