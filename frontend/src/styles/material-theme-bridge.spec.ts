// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };
const frontendDir = nodeGlobal.process.cwd();
const stylesDir = join(frontendDir, 'src', 'styles');
const bridgePath = join(stylesDir, '_material-theme-bridge.scss');
const rootStylesPath = join(frontendDir, 'src', 'styles.scss');
const packageJsonPath = join(frontendDir, 'package.json');
const lockfilePath = join(frontendDir, 'package-lock.json');
const policyPath = join(frontendDir, 'dependency-policy.json');
const tokensPath = join(stylesDir, '_tokens.scss');
const accessibilityPath = join(stylesDir, '_accessibility.scss');

function readText(path: string): string {
  return readFileSync(path, 'utf8');
}

function readJson(path: string): Record<string, unknown> {
  return JSON.parse(readText(path)) as Record<string, unknown>;
}

describe('Angular Material theme bridge', () => {
  it('pins @angular/material and @angular/cdk to exact 22.1.6 without @angular/animations', () => {
    const packageJson = readJson(packageJsonPath);
    const dependencies = packageJson['dependencies'] as Record<string, string>;

    expect(dependencies['@angular/material']).toBe('22.1.6');
    expect(dependencies['@angular/cdk']).toBe('22.1.6');
    expect(dependencies['@angular/animations']).toBeUndefined();
  });

  it('records the exact Material and CDK versions in the lockfile', () => {
    const lockfile = readJson(lockfilePath);
    const packages = lockfile['packages'] as Record<string, { version?: string }>;

    expect(packages['node_modules/@angular/material']?.version).toBe('22.1.6');
    expect(packages['node_modules/@angular/cdk']?.version).toBe('22.1.6');
  });

  it('approves Material and CDK as exact direct dependencies in the dependency policy', () => {
    const policy = readJson(policyPath);
    const approved = (policy['approvedDirectDependencies'] as Record<string, Record<string, string>>)['dependencies'];
    const denied = policy['deniedDirectDependencies'] as Record<string, string>;

    expect(approved['@angular/material']).toBe('22.1.6');
    expect(approved['@angular/cdk']).toBe('22.1.6');
    expect(denied['@angular/material']).toBeUndefined();
    expect(denied['@angular/cdk']).toBeUndefined();
  });

  it('defines the centralized bridge using M2 APIs only', () => {
    const bridge = readText(bridgePath);

    expect(bridge).toMatch(/@use\s+['"]@angular\/material['"]\s+as\s+mat/);
    expect(bridge).toContain('mat.m2-define-palette(');
    expect(bridge).toContain('mat.m2-define-light-theme(');
    expect(bridge).toContain('mat.all-component-themes(');
  });

  it('imports the token source through the bridge', () => {
    const bridge = readText(bridgePath);

    expect(bridge).toMatch(/@use\s+['"]tokens['"]/);
  });

  it('emits exactly one light theme with no dark theme, custom typography, or custom density', () => {
    const bridge = readText(bridgePath);

    expect(bridge).toContain('mat.m2-define-light-theme(');
    expect(bridge).not.toContain('m2-define-dark-theme');
    expect(bridge).not.toMatch(/typography-config/);
    expect(bridge).not.toMatch(/density\s*:/);
  });

  it('maps Material roles to the exact approved colors', () => {
    const bridge = readText(bridgePath);

    expect(bridge).toContain('#006B66');
    expect(bridge).toContain('#173B57');
    expect(bridge).toContain('#B3261E');
  });

  it('keeps warning and exam/focus semantics separate from Material roles', () => {
    const bridge = readText(bridgePath);

    expect(bridge).not.toContain('#8A4B00');
    expect(bridge).not.toContain('#4F46B8');
  });

  it('uses only approved colors in the bridge without synthetic palette colors', () => {
    const bridge = readText(bridgePath);
    const approvedColors = new Set(['#006B66', '#173B57', '#B3261E', '#FFFFFF']);
    const hexMatches = bridge.match(/#[0-9A-Fa-f]{6}\b/g) ?? [];

    expect(hexMatches.length).toBeGreaterThan(0);

    for (const hex of hexMatches) {
      expect(approvedColors.has(hex.toUpperCase())).toBe(true);
    }
  });

  it('integrates the bridge only through styles.scss', () => {
    const root = readText(rootStylesPath);

    expect(root).toMatch(/@use\s+['"]styles\/material-theme-bridge['"]/);
  });

  it('leaves the verified focus foundation unchanged', () => {
    const tokens = readText(tokensPath);
    const accessibility = readText(accessibilityPath);

    expect(tokens).toMatch(/--np-focus-ring-color\s*:\s*var\(--np-color-brand-1\)/);
    expect(tokens).toContain('--np-focus-ring-width: 2px');
    expect(tokens).toContain('--np-focus-ring-offset: 4px');
    expect(accessibility).toMatch(/:focus-visible\s*\{/);
    expect(accessibility).toMatch(
      /outline\s*:\s*var\(--np-focus-ring-width\)\s+solid\s+var\(--np-focus-ring-color\)/,
    );
  });
});
