import { describe, expect, it } from 'vitest';
import { TwoStepConfirmation } from './confirmation';

describe('TwoStepConfirmation', () => {
  it('starts idle and cannot execute', () => {
    const confirmation = new TwoStepConfirmation();

    expect(confirmation.state).toBe('idle');
    expect(confirmation.canExecute).toBe(false);
  });

  it('does not authorize execution without an explicit request first', () => {
    const confirmation = new TwoStepConfirmation();

    expect(confirmation.confirm()).toBe(false);
    expect(confirmation.canExecute).toBe(false);
  });

  it('arms confirmation on request without executing', () => {
    const confirmation = new TwoStepConfirmation();

    confirmation.request();

    expect(confirmation.state).toBe('confirming');
    expect(confirmation.canExecute).toBe(true);
  });

  it('authorizes exactly one execution on confirm and resets to idle', () => {
    const confirmation = new TwoStepConfirmation();
    confirmation.request();

    expect(confirmation.confirm()).toBe(true);
    expect(confirmation.state).toBe('idle');
    expect(confirmation.canExecute).toBe(false);
    expect(confirmation.confirm()).toBe(false);
  });

  it('cancels back to idle without authorizing execution', () => {
    const confirmation = new TwoStepConfirmation();
    confirmation.request();

    confirmation.cancel();

    expect(confirmation.state).toBe('idle');
    expect(confirmation.canExecute).toBe(false);
    expect(confirmation.confirm()).toBe(false);
  });
});
