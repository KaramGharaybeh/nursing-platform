import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseEducation } from './nurse-education';

const COUNTRIES = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const RECORDS = [
  {
    id: 'edu-1',
    institutionName: 'Harbor Institute',
    degree: 'Master of Nursing',
    fieldOfStudy: 'Nursing Education',
    countryId: null,
    countryName: null,
    startDate: '2023-09-01',
    endDate: null,
    description:
      'Advanced study in clinical teaching and curriculum design for nursing programs, with supervised practicum hours, simulation lab work, and a capstone research project on competency assessment.',
  },
  {
    id: 'edu-2',
    institutionName: 'Riyadh College of Nursing',
    degree: 'Bachelor of Science in Nursing',
    fieldOfStudy: null,
    countryId: 'country-sa',
    countryName: 'Saudi Arabia',
    startDate: '2015-09-01',
    endDate: '2019-06-30',
    description: null,
  },
  {
    id: 'edu-3',
    institutionName: 'Harbor Institute',
    degree: 'Diploma in Wound Care',
    fieldOfStudy: null,
    countryId: null,
    countryName: null,
    startDate: null,
    endDate: null,
    description: null,
  },
];

class PopulatedApi {
  listEducation() {
    return of(RECORDS);
  }
  createEducation() {
    return of(RECORDS[1]);
  }
  updateEducation() {
    return of(RECORDS[0]);
  }
  deleteEducation() {
    return of(undefined);
  }
}

class EmptyApi extends PopulatedApi {
  override listEducation() {
    return of([]);
  }
}

class CountriesApiStub {
  list() {
    return of(COUNTRIES);
  }
}

function providers(api: PopulatedApi) {
  return [
    provideRouter([]),
    { provide: NurseProfileApi, useValue: api },
    { provide: CountriesApi, useValue: new CountriesApiStub() },
  ];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const meta: Meta<NurseEducation> = {
  component: NurseEducation,
  title: 'Features/Nurse/Education',
};
export default meta;
type Story = StoryObj<NurseEducation>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const AddEducation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const add = canvasElement.querySelector<HTMLElement>('[data-testid="add-education"]');
    if (add === null) {
      throw new Error('Add education story could not find the Add education action.');
    }
    add.click();
    await settle();
  },
};

export const EditEducation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const edit = canvasElement.querySelector<HTMLElement>('[data-testid="education-edit"]');
    if (edit === null) {
      throw new Error('Edit education story could not find an Edit action.');
    }
    edit.click();
    await settle();
  },
};

export const DeleteConfirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const remove = canvasElement.querySelector<HTMLElement>('[data-testid="education-delete"]');
    if (remove === null) {
      throw new Error('Delete confirmation story could not find a Delete action.');
    }
    remove.click();
    await settle();
  },
};
