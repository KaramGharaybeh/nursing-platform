import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { ExamsApi } from '../../core/api/exams-api';
import type { ExamReview } from '../../core/api/exams-api';
import { ExamReviewScreen } from './exam-review';

function correctReview(): ExamReview {
  return {
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    items: [
      {
        displayOrder: 1,
        text: 'Which action comes first in an emergency?',
        explanation: 'Scene safety always comes first.',
        points: 2,
        pointsEarned: 2,
        options: [
          { displayOrder: 1, text: 'Ensure scene safety', isCorrect: true, isSelected: true },
          { displayOrder: 2, text: 'Start compressions', isCorrect: false, isSelected: false },
        ],
      },
    ],
  };
}

function incorrectReview(): ExamReview {
  return {
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    items: [
      {
        displayOrder: 1,
        text: 'Which action comes first in an emergency?',
        explanation: 'Scene safety always comes first.',
        points: 2,
        pointsEarned: 0,
        options: [
          { displayOrder: 1, text: 'Ensure scene safety', isCorrect: true, isSelected: false },
          { displayOrder: 2, text: 'Start compressions', isCorrect: false, isSelected: true },
        ],
      },
    ],
  };
}

function unansweredReview(): ExamReview {
  return {
    examId: 'exam-1',
    examTitle: 'NCLEX Readiness',
    status: 'Submitted',
    items: [
      {
        displayOrder: 1,
        text: 'Which action comes first in an emergency?',
        explanation: null,
        points: 2,
        pointsEarned: 0,
        options: [
          { displayOrder: 1, text: 'Ensure scene safety', isCorrect: true, isSelected: false },
          { displayOrder: 2, text: 'Start compressions', isCorrect: false, isSelected: false },
        ],
      },
    ],
  };
}

class CorrectApi {
  getExamSessionReview() {
    return of(correctReview());
  }
}

class IncorrectApi {
  getExamSessionReview() {
    return of(incorrectReview());
  }
}

class UnansweredApi {
  getExamSessionReview() {
    return of(unansweredReview());
  }
}

class ExpiredApi {
  getExamSessionReview() {
    return of({ ...incorrectReview(), status: 'Expired' });
  }
}

class UnavailableApi {
  getExamSessionReview() {
    return throwError(() => ({ status: 404 }));
  }
}

class NotFinalizedApi {
  getExamSessionReview() {
    return throwError(() => ({ status: 409 }));
  }
}

class LoadErrorApi {
  getExamSessionReview() {
    return throwError(() => ({ status: 500 }));
  }
}

function providers(api: unknown) {
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

const meta: Meta<ExamReviewScreen> = {
  component: ExamReviewScreen,
  title: 'Features/Exams/ExamReview',
};
export default meta;
type Story = StoryObj<ExamReviewScreen>;

export const Correct: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new CorrectApi()) })],
};

export const Incorrect: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new IncorrectApi()) })],
};

export const Unanswered: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnansweredApi()) })],
};

export const Expired: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExpiredApi()) })],
};

export const Unavailable: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new UnavailableApi()) })],
};

export const NotFinalized: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new NotFinalizedApi()) })],
};

export const LoadError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new LoadErrorApi()) })],
};
