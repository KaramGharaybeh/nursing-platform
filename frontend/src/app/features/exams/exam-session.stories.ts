import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import { ExamSessionScreen } from './exam-session';

function items(answered: boolean) {
  return [
    {
      examSessionQuestionId: 'q-1',
      text: 'First prompt',
      points: 1,
      displayOrder: 1,
      selectedExamSessionAnswerOptionId: answered ? 'q-1-a' : null,
      options: [
        { examSessionAnswerOptionId: 'q-1-a', text: 'First option one', displayOrder: 1 },
        { examSessionAnswerOptionId: 'q-1-b', text: 'First option two', displayOrder: 2 },
      ],
    },
    {
      examSessionQuestionId: 'q-2',
      text: 'Second prompt',
      points: 1,
      displayOrder: 2,
      selectedExamSessionAnswerOptionId: null,
      options: [
        { examSessionAnswerOptionId: 'q-2-a', text: 'Second option one', displayOrder: 1 },
        { examSessionAnswerOptionId: 'q-2-b', text: 'Second option two', displayOrder: 2 },
      ],
    },
  ];
}

function sessionBody(status: string, remainingSeconds: number, answered: boolean) {
  return {
    id: 'session-9',
    examId: 'exam-1',
    examTitle: 'Exam',
    status,
    startedAt: '2026-09-18T00:00:00Z',
    expiresAt: '2999-01-01T00:00:00Z',
    remainingSeconds,
    items: items(answered),
  };
}

class ActiveApi {
  remainingSeconds = 3600;
  answered = false;

  getExamSession() {
    return of(sessionBody('InProgress', this.remainingSeconds, this.answered));
  }

  saveExamSessionAnswers() {
    this.answered = true;
    return of(sessionBody('InProgress', this.remainingSeconds, true));
  }

  submitExamSession() {
    return of({
      score: 1,
      maxScore: 2,
      percentage: 50,
      passed: false,
      correctCount: 1,
      questionCount: 2,
    });
  }
}

class NearExpiryApi extends ActiveApi {
  constructor() {
    super();
    this.remainingSeconds = 299;
  }
}

class ExpiredApi extends ActiveApi {
  override getExamSession() {
    return of(sessionBody('Expired', 0, true));
  }
}

class MissingApi extends ActiveApi {
  override getExamSession() {
    return throwError(() => ({ status: 404 }));
  }
}

function providers(api: ActiveApi) {
  return [
    provideRouter([]),
    { provide: ExamsApi, useValue: api },
    {
      provide: ActivatedRoute,
      useValue: {
        snapshot: { paramMap: convertToParamMap({ examId: 'exam-1', sessionId: 'session-9' }) },
      },
    },
  ];
}

const meta: Meta<ExamSessionScreen> = {
  component: ExamSessionScreen,
  title: 'Features/Exams/ExamSession',
};
export default meta;
type Story = StoryObj<ExamSessionScreen>;

export const Unanswered: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ActiveApi()) })],
};

export const NearExpiry: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NearExpiryApi()) })],
};

export const Expired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExpiredApi()) })],
};

export const Missing: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new MissingApi()) })],
};
