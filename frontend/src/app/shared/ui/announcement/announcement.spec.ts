import { describe, expect, it } from 'vitest';
import { Announcer } from './announcement';

describe('Announcer', () => {
  async function announced(announcer: Announcer, text: string, politeness: 'polite' | 'assertive' = 'polite') {
    announcer.announce(text, politeness);
    await Promise.resolve();
    return announcer.current();
  }

  it('starts with no current announcement', () => {
    const announcer = new Announcer();

    expect(announcer.current()).toBeUndefined();
  });

  it('announces a polite message by default', async () => {
    const announcer = new Announcer();

    const current = await announced(announcer, 'Experience deleted.');

    expect(current?.text).toBe('Experience deleted.');
    expect(current?.politeness).toBe('polite');
  });

  it('announces an assertive message when requested', async () => {
    const announcer = new Announcer();

    const current = await announced(announcer, 'Delete failed. Try again.', 'assertive');

    expect(current?.text).toBe('Delete failed. Try again.');
    expect(current?.politeness).toBe('assertive');
  });

  it('ignores empty or whitespace-only announcements', async () => {
    const announcer = new Announcer();
    await announced(announcer, 'Experience deleted.');

    announcer.announce('   ');
    await Promise.resolve();

    expect(announcer.current()?.text).toBe('Experience deleted.');
  });

  it('re-announces an identical message by advancing the sequence', async () => {
    const announcer = new Announcer();
    const first = await announced(announcer, 'Experience deleted.');

    const second = await announced(announcer, 'Experience deleted.');

    expect(second?.text).toBe('Experience deleted.');
    expect(second?.sequence).toBeGreaterThan(first?.sequence ?? 0);
  });

  it('clears the current announcement', async () => {
    const announcer = new Announcer();
    await announced(announcer, 'Experience deleted.');

    announcer.clear();

    expect(announcer.current()).toBeUndefined();
  });
});
