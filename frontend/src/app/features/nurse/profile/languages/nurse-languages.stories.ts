import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { LanguagesApi } from '../../../../core/api/languages-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseLanguages } from './nurse-languages';

const CATALOG = [
  { id: 'lang-ar', name: 'Arabic', code: 'AR' },
  { id: 'lang-en', name: 'English', code: 'EN' },
  { id: 'lang-fr', name: 'French', code: 'FR' },
];

const SAVED = [
  { id: 'nl-1', languageId: 'lang-en', name: 'English', code: 'EN', proficiency: 'Fluent' },
  { id: 'nl-2', languageId: 'lang-ar', name: 'Arabic', code: 'AR', proficiency: 'Native' },
];

class PopulatedApi {
  listLanguages() {
    return of(SAVED);
  }
  updateLanguages() {
    return of(SAVED);
  }
}

class EmptyApi extends PopulatedApi {
  override listLanguages() {
    return of([]);
  }
}

class LanguagesApiStub {
  list() {
    return of(CATALOG);
  }
}

function providers(api: PopulatedApi) {
  return [
    provideRouter([]),
    { provide: LanguagesApi, useValue: new LanguagesApiStub() },
    { provide: NurseProfileApi, useValue: api },
  ];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<NurseLanguages> = {
  component: NurseLanguages,
  title: 'Features/Nurse/Languages',
};
export default meta;
type Story = StoryObj<NurseLanguages>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const ValidationState: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const add = canvasElement.querySelector<HTMLElement>('[data-testid="add-language"]');
    if (add === null) {
      throw new Error('Languages validation story could not find the Add language action.');
    }
    add.click();
    await settle();
    const save = canvasElement.querySelector<HTMLElement>('[data-testid="save-languages"]');
    if (save === null) {
      throw new Error('Languages validation story could not find the Save action.');
    }
    save.click();
    await settle();
  },
};
