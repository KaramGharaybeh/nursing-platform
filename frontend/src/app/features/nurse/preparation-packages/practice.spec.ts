import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../app.routes';
import { PreparationPackagePracticeApi } from '../../../core/api/preparation-package-practice-api';
import type { PackagePracticeAnswerSubmissionDto } from '../../../core/api/generated/models/package-practice-answer-submission-dto';
import type { PackagePracticeContentListDto } from '../../../core/api/generated/models/package-practice-content-list-dto';
import type { PackagePracticeProgressSummaryDto } from '../../../core/api/generated/models/package-practice-progress-summary-dto';
import { Practice } from './practice';

const ITEM_1 = '11111111-1111-4111-8111-111111111111';
const ITEM_2 = '22222222-2222-4222-8222-222222222222';
const OPT_1A = 'a1a1a1a1-1111-4111-8111-a1a1a1a1a1a1';
const OPT_1B = 'b1b1b1b1-1111-4111-8111-b1b1b1b1b1b1';
const OPT_2A = 'a2a2a2a2-2222-4222-8222-a2a2a2a2a2a2';
const OPT_2B = 'b2b2b2b2-2222-4222-8222-b2b2b2b2b2b2';

function content(): PackagePracticeContentListDto {
  return {
    packagePurchaseEntitlementId: 'ent-1',
    practiceCollectionVersionId: 'ver-1',
    totalItems: 2,
    items: [
      {
        practiceItemId: ITEM_1,
        displayOrder: 1,
        prompt: 'First practice prompt',
        answerOptions: [
          { practiceAnswerOptionId: OPT_1A, optionText: 'First option one', displayOrder: 1 },
          { practiceAnswerOptionId: OPT_1B, optionText: 'First option two', displayOrder: 2 },
        ],
      },
      {
        practiceItemId: ITEM_2,
        displayOrder: 2,
        prompt: 'Second practice prompt',
        answerOptions: [
          { practiceAnswerOptionId: OPT_2A, optionText: 'Second option one', displayOrder: 1 },
          { practiceAnswerOptionId: OPT_2B, optionText: 'Second option two', displayOrder: 2 },
        ],
      },
    ],
  };
}

function progress(item1State: number, item2State: number): PackagePracticeProgressSummaryDto {
  const answered = [item1State, item2State].filter((s) => s !== 0).length;
  const correct = [item1State, item2State].filter((s) => s === 1).length;
  return {
    packagePurchaseEntitlementId: 'ent-1',
    practiceCollectionVersionId: 'ver-1',
    totalItems: 2,
    answeredCount: answered,
    unansweredCount: 2 - answered,
    correctCount: correct,
    incorrectCount: answered - correct,
    itemStates: [
      {
        practiceItemId: ITEM_1,
        state: item1State,
        selectedPracticeAnswerOptionId: item1State === 0 ? null : OPT_1A,
        lastAnsweredAt: item1State === 0 ? null : '2026-09-18T00:00:00Z',
      },
      {
        practiceItemId: ITEM_2,
        state: item2State,
        selectedPracticeAnswerOptionId: null,
        lastAnsweredAt: null,
      },
    ],
  };
}

class PracticeApiStub {
  items: PackagePracticeContentListDto = content();
  progressValue: PackagePracticeProgressSummaryDto = progress(1, 0);
  itemsError: unknown = undefined;
  progressError: unknown = undefined;
  submitResult: PackagePracticeAnswerSubmissionDto = {
    practiceItemId: ITEM_2,
    state: 1,
    selectedPracticeAnswerOptionId: OPT_2A,
    lastAnsweredAt: '2026-09-18T01:00:00Z',
    immediateFeedback: 'Feedback for the second prompt',
  };
  submitError: unknown = undefined;
  submitted: { entitlementId: string; itemId: string; optionId: string }[] = [];
  progressCalls = 0;

  getItems() {
    if (this.itemsError !== undefined) {
      return throwError(() => this.itemsError);
    }
    return of(this.items);
  }

  getProgress() {
    this.progressCalls += 1;
    if (this.progressError !== undefined) {
      return throwError(() => this.progressError);
    }
    return of(this.progressValue);
  }

  submitAnswer(entitlementId: string, itemId: string, optionId: string) {
    this.submitted.push({ entitlementId, itemId, optionId });
    if (this.submitError !== undefined) {
      return throwError(() => this.submitError);
    }
    this.progressValue = {
      ...this.progressValue,
      answeredCount: 0,
      unansweredCount: 0,
      correctCount: 0,
      incorrectCount: 0,
      itemStates: this.progressValue.itemStates.map((entry) =>
        entry.practiceItemId === itemId
          ? {
              ...entry,
              state: this.submitResult.state,
              selectedPracticeAnswerOptionId: optionId,
              lastAnsweredAt: this.submitResult.lastAnsweredAt,
            }
          : entry,
      ),
    };
    const states = this.progressValue.itemStates.map((entry) => entry.state);
    const answered = states.filter((state) => state !== 0).length;
    const correct = states.filter((state) => state === 1).length;
    this.progressValue = {
      ...this.progressValue,
      answeredCount: answered,
      unansweredCount: this.progressValue.totalItems - answered,
      correctCount: correct,
      incorrectCount: answered - correct,
    };
    return of(this.submitResult);
  }
}

async function settle(fixture: ComponentFixture<Practice>): Promise<void> {
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
}

async function setup(stub?: PracticeApiStub): Promise<{ fixture: ComponentFixture<Practice>; api: PracticeApiStub }> {
  const api = stub ?? new PracticeApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [Practice],
    providers: [
      provideRouter([]),
      { provide: PreparationPackagePracticeApi, useValue: api },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap({ entitlementId: 'ent-1' }) } },
      },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(Practice);
  fixture.detectChanges();
  await settle(fixture);
  return { fixture, api };
}

function text(fixture: ComponentFixture<Practice>): string {
  return ((fixture.nativeElement as HTMLElement).textContent ?? '');
}

function byTestId(fixture: ComponentFixture<Practice>, id: string): HTMLElement | null {
  return (fixture.nativeElement as HTMLElement).querySelector(`[data-testid="${id}"]`);
}

const UUID_PATTERN = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

describe('Practice (T-FE-078)', () => {
  it('loads items and progress and resumes at the first unanswered item', async () => {
    const { fixture } = await setup();

    expect(byTestId(fixture, 'practice-position')?.textContent).toContain('Question 2 of 2');
    expect(byTestId(fixture, 'practice-prompt')?.textContent).toContain('Second practice prompt');
  });

  it('enters the completion state when every item is answered while keeping re-answer available', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(1, 2);
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'practice-complete-notice')?.textContent).toContain('Practice complete');
    expect(byTestId(fixture, 'practice-submit')).not.toBeNull();
    expect(byTestId(fixture, 'practice-prompt')?.textContent).toContain('First practice prompt');
  });

  it('shows submitted feedback on the completion state', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(0, 0);
    stub.submitResult = {
      practiceItemId: ITEM_1,
      state: 1,
      selectedPracticeAnswerOptionId: OPT_1A,
      lastAnsweredAt: '2026-09-18T01:00:00Z',
      immediateFeedback: 'Feedback for the first prompt',
    };
    const { fixture } = await setup(stub);
    const element = fixture.nativeElement as HTMLElement;

    (byTestId(fixture, 'practice-option')?.querySelector('input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    stub.submitResult = {
      practiceItemId: ITEM_2,
      state: 2,
      selectedPracticeAnswerOptionId: OPT_2A,
      lastAnsweredAt: '2026-09-18T02:00:00Z',
      immediateFeedback: 'Feedback for the second prompt',
    };
    (byTestId(fixture, 'practice-next') as HTMLButtonElement).click();
    fixture.detectChanges();
    (element.querySelector('[data-testid="practice-option"] input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(byTestId(fixture, 'practice-complete-notice')).not.toBeNull();
    expect(byTestId(fixture, 'practice-feedback')?.textContent).toContain(
      'Feedback for the second prompt',
    );
  });

  it('renders options in backend order without visible ids or correct-option reveal', async () => {
    const { fixture } = await setup();
    const element = fixture.nativeElement as HTMLElement;
    const options = [...element.querySelectorAll('[data-testid="practice-option"]')].map((o) =>
      o.textContent?.trim(),
    );

    expect(options).toEqual(['Second option one', 'Second option two']);
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
    expect(byTestId(fixture, 'practice-feedback')).toBeNull();
    expect(byTestId(fixture, 'practice-correct-notice')).toBeNull();
    expect(byTestId(fixture, 'practice-incorrect-notice')).toBeNull();
  });

  it('keeps submit disabled until an option is selected', async () => {
    const { fixture } = await setup();
    const submit = byTestId(fixture, 'practice-submit') as HTMLButtonElement | null;

    expect(submit?.disabled).toBe(true);

    const firstOption = (fixture.nativeElement as HTMLElement).querySelector(
      '[data-testid="practice-option"] input',
    ) as HTMLInputElement;
    firstOption.click();
    fixture.detectChanges();

    expect((byTestId(fixture, 'practice-submit') as HTMLButtonElement).disabled).toBe(false);
  });

  it('shows a correct notice with feedback after a correct submission', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(0, 0);
    stub.submitResult = {
      practiceItemId: ITEM_1,
      state: 1,
      selectedPracticeAnswerOptionId: OPT_1A,
      lastAnsweredAt: '2026-09-18T01:00:00Z',
      immediateFeedback: 'Feedback for the first prompt',
    };
    const { fixture, api } = await setup(stub);

    (byTestId(fixture, 'practice-option')?.querySelector('input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(api.submitted).toEqual([{ entitlementId: 'ent-1', itemId: ITEM_1, optionId: OPT_1A }]);
    expect(byTestId(fixture, 'practice-correct-notice')).not.toBeNull();
    expect(byTestId(fixture, 'practice-incorrect-notice')).toBeNull();
    expect(byTestId(fixture, 'practice-feedback')?.textContent).toContain(
      'Feedback for the first prompt',
    );
  });

  it('shows an incorrect notice with feedback without revealing the correct option', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(0, 0);
    stub.submitResult = {
      practiceItemId: ITEM_1,
      state: 2,
      selectedPracticeAnswerOptionId: OPT_1A,
      lastAnsweredAt: '2026-09-18T01:00:00Z',
      immediateFeedback: 'Feedback for the first prompt',
    };
    const { fixture } = await setup(stub);

    (byTestId(fixture, 'practice-option')?.querySelector('input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(byTestId(fixture, 'practice-incorrect-notice')).not.toBeNull();
    expect(byTestId(fixture, 'practice-correct-notice')).toBeNull();
    expect(byTestId(fixture, 'practice-feedback')?.textContent).toContain(
      'Feedback for the first prompt',
    );
    expect(text(fixture)).not.toMatch(/correct option is/i);
  });

  it('restores the previously selected option for an answered item and allows re-answer', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(0, 0);
    const { fixture, api } = await setup(stub);
    const element = fixture.nativeElement as HTMLElement;

    (element.querySelector('[data-testid="practice-option"] input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    const inputs = [...element.querySelectorAll('[data-testid="practice-option"] input')] as HTMLInputElement[];
    inputs[1].click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.submitted.length).toBe(2);
    expect(api.submitted[1]).toEqual({ entitlementId: 'ent-1', itemId: ITEM_1, optionId: OPT_1B });
  });

  it('refreshes progress counts after a submission', async () => {
    const { fixture, api } = await setup();
    const callsAfterLoad = api.progressCalls;

    (byTestId(fixture, 'practice-option')?.querySelector('input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));

    expect(api.progressCalls).toBeGreaterThan(callsAfterLoad);
  });

  it('shows a non-revealing missing notice with a back link for unknown entitlements', async () => {
    const stub = new PracticeApiStub();
    stub.itemsError = { status: 404 };
    stub.progressError = { status: 404 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'practice-missing-notice')).not.toBeNull();
    expect(byTestId(fixture, 'practice-back-link')).not.toBeNull();
    expect(text(fixture)).not.toMatch(UUID_PATTERN);
  });

  it('retries a recoverable error while preserving the current item', async () => {
    const stub = new PracticeApiStub();
    stub.progressError = { status: 500 };
    const { fixture, api } = await setup(stub);
    const component = fixture.componentInstance as unknown as { retry(): Promise<void> };

    expect(text(fixture)).toContain('could not be loaded');

    api.progressError = undefined;
    await component.retry();
    await settle(fixture);

    expect(byTestId(fixture, 'practice-position')?.textContent).toContain('Question 2 of 2');
  });

  it('moves to the access-ended state when submission returns 409', async () => {
    const stub = new PracticeApiStub();
    stub.progressValue = progress(0, 0);
    stub.submitError = { status: 409 };
    const { fixture } = await setup(stub);

    (byTestId(fixture, 'practice-option')?.querySelector('input') as HTMLInputElement).click();
    fixture.detectChanges();
    (byTestId(fixture, 'practice-submit') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(byTestId(fixture, 'practice-access-ended-notice')).not.toBeNull();
    expect(byTestId(fixture, 'practice-submit')).toBeNull();
  });

  it('transitions to the read-only access-ended state on 409 without submission controls', async () => {    const stub = new PracticeApiStub();
    stub.itemsError = { status: 409 };
    const { fixture } = await setup(stub);

    expect(byTestId(fixture, 'practice-access-ended-notice')?.textContent).toContain(
      'Practice access is no longer available. Your previous progress is shown below.',
    );
    expect(byTestId(fixture, 'practice-submit')).toBeNull();
    expect(byTestId(fixture, 'practice-prompt')).toBeNull();
    expect(byTestId(fixture, 'practice-progress-summary')?.textContent).toContain('1 of 2 answered');
  });
});

describe('Practice route', () => {
  it('mounts /nurse/preparation-packages/:entitlementId/practice with the three-guard pattern and routeId', () => {
    const route = routes.find(
      (entry) => entry.path === 'nurse/preparation-packages/:entitlementId/practice',
    );

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'PREPARATION_PACKAGES_PRACTICE' });
  });
});
