import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseSkills } from './nurse-skills';

const SKILLS = [
  { id: 'skill-1', name: 'Triage' },
  { id: 'skill-2', name: 'Critical Care' },
  { id: 'skill-3', name: 'Wound Care' },
];

class PopulatedApi {
  listSkills() {
    return of(SKILLS);
  }
  updateSkills(request: { skills: string[] }) {
    return of(request.skills.map((name, index) => ({ id: `skill-${index}`, name })));
  }
}

class EmptyApi extends PopulatedApi {
  override listSkills() {
    return of([]);
  }
}

function providers(api: PopulatedApi) {
  return [provideRouter([]), { provide: NurseProfileApi, useValue: api }];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<NurseSkills> = {
  component: NurseSkills,
  title: 'Features/Nurse/Skills',
};
export default meta;
type Story = StoryObj<NurseSkills>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const EditingValidation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const input = canvasElement.querySelector<HTMLInputElement>('#nurse-skills-input');
    const add = canvasElement.querySelector<HTMLElement>('[data-testid="add-skill"]');
    if (input === null || add === null) {
      throw new Error('Skills editing story could not find the skill input or Add action.');
    }
    input.value = 'triage';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    add.click();
    await settle();
  },
};
