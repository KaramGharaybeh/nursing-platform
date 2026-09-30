import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { PreparationPackagePracticeApi } from '../../../core/api/preparation-package-practice-api';
import { Practice } from './practice';

const ITEMS = {
  packagePurchaseEntitlementId: 'ent-1',
  practiceCollectionVersionId: 'ver-1',
  totalItems: 2,
  items: [
    {
      practiceItemId: 'item-1',
      displayOrder: 1,
      prompt: 'First practice prompt',
      answerOptions: [
        { practiceAnswerOptionId: 'opt-1a', optionText: 'First option one', displayOrder: 1 },
        { practiceAnswerOptionId: 'opt-1b', optionText: 'First option two', displayOrder: 2 },
      ],
    },
    {
      practiceItemId: 'item-2',
      displayOrder: 2,
      prompt: 'Second practice prompt',
      answerOptions: [
        { practiceAnswerOptionId: 'opt-2a', optionText: 'Second option one', displayOrder: 1 },
        { practiceAnswerOptionId: 'opt-2b', optionText: 'Second option two', displayOrder: 2 },
      ],
    },
  ],
};

const FRESH_PROGRESS = {
  packagePurchaseEntitlementId: 'ent-1',
  practiceCollectionVersionId: 'ver-1',
  totalItems: 2,
  answeredCount: 0,
  unansweredCount: 2,
  correctCount: 0,
  incorrectCount: 0,
  itemStates: [
    { practiceItemId: 'item-1', state: 0, selectedPracticeAnswerOptionId: null, lastAnsweredAt: null },
    { practiceItemId: 'item-2', state: 0, selectedPracticeAnswerOptionId: null, lastAnsweredAt: null },
  ],
};

const COMPLETE_PROGRESS = {
  ...FRESH_PROGRESS,
  answeredCount: 2,
  unansweredCount: 0,
  correctCount: 1,
  incorrectCount: 1,
  itemStates: [
    {
      practiceItemId: 'item-1',
      state: 1,
      selectedPracticeAnswerOptionId: 'opt-1a',
      lastAnsweredAt: '2026-09-18T00:00:00Z',
    },
    {
      practiceItemId: 'item-2',
      state: 2,
      selectedPracticeAnswerOptionId: 'opt-2b',
      lastAnsweredAt: '2026-09-18T01:00:00Z',
    },
  ],
};

class ActiveApi {
  getItems() {
    return of(ITEMS);
  }

  getProgress() {
    return of(FRESH_PROGRESS);
  }

  submitAnswer() {
    return of({
      practiceItemId: 'item-1',
      state: 1,
      selectedPracticeAnswerOptionId: 'opt-1a',
      lastAnsweredAt: '2026-09-18T02:00:00Z',
      immediateFeedback: 'Feedback for the first prompt',
    });
  }
}

class CompleteApi extends ActiveApi {
  override getProgress() {
    return of(COMPLETE_PROGRESS);
  }
}

class AccessEndedApi extends ActiveApi {
  override getItems() {
    return throwError(() => ({ status: 409 }));
  }
}

class MissingApi extends ActiveApi {
  override getItems() {
    return throwError(() => ({ status: 404 }));
  }

  override getProgress() {
    return throwError(() => ({ status: 404 }));
  }
}

function providers(api: ActiveApi) {
  return [
    provideRouter([]),
    { provide: PreparationPackagePracticeApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: { snapshot: { paramMap: convertToParamMap({ entitlementId: 'ent-1' }) } },
    },
  ];
}

const meta: Meta<Practice> = {
  component: Practice,
  title: 'Features/NursePreparationPackages/Practice',
};
export default meta;
type Story = StoryObj<Practice>;

export const Active: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ActiveApi()) })],
};

export const Complete: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new CompleteApi()) })],
};

export const AccessEnded: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new AccessEndedApi()) })],
};

export const Missing: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingApi()) })],
};
