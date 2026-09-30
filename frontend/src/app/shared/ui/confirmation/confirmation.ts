/**
 * Shared two-step destructive-confirmation semantics (T-FE-036).
 *
 * The owning feature renders its own confirmation UI inline (no modal/dialog);
 * this helper owns only the state contract so Education/Certificates can reuse
 * the exact same semantics without copying logic:
 *
 * - `request()` moves `idle` -> `confirming` (first destructive click arms only).
 * - `confirm()` returns true only from `confirming`, then resets to `idle`.
 * - `cancel()` returns to `idle` without ever authorizing execution.
 * - `canExecute` is true only while `confirming`; the feature must call the
 *   backend exclusively behind `confirm() === true`.
 */

export type TwoStepConfirmationState = 'idle' | 'confirming';

export class TwoStepConfirmation {
  private confirmationState: TwoStepConfirmationState = 'idle';

  get state(): TwoStepConfirmationState {
    return this.confirmationState;
  }

  get canExecute(): boolean {
    return this.confirmationState === 'confirming';
  }

  request(): void {
    this.confirmationState = 'confirming';
  }

  cancel(): void {
    this.confirmationState = 'idle';
  }

  confirm(): boolean {
    if (this.confirmationState !== 'confirming') {
      return false;
    }
    this.confirmationState = 'idle';
    return true;
  }
}
