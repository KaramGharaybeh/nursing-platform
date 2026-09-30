import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of } from 'rxjs';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseExperience } from './nurse-experience';

const COUNTRIES = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const EXPERIENCES = [
  {
    id: 'exp-1',
    facilityName: 'City General Hospital',
    jobTitle: 'Emergency Department Nurse',
    countryId: 'country-sa',
    countryName: 'Saudi Arabia',
    startDate: '2019-04-01',
    endDate: null,
    isCurrent: true,
    description:
      'Triage and resuscitation across a busy urban emergency department with high-acuity intake, rapid assessment, and coordination with physicians and specialists around the clock.',
  },
  {
    id: 'exp-2',
    facilityName: 'Riverside Clinic',
    jobTitle: 'Staff Nurse',
    countryId: null,
    countryName: null,
    startDate: '2016-01-15',
    endDate: '2019-03-30',
    isCurrent: false,
    description: null,
  },
];

class PopulatedApi {
  listExperiences() {
    return of(EXPERIENCES);
  }
  createExperience() {
    return of(EXPERIENCES[0]);
  }
  updateExperience() {
    return of(EXPERIENCES[0]);
  }
  deleteExperience() {
    return of(undefined);
  }
}

class EmptyApi extends PopulatedApi {
  override listExperiences() {
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

const meta: Meta<NurseExperience> = {
  component: NurseExperience,
  title: 'Features/Nurse/Experience',
};
export default meta;
type Story = StoryObj<NurseExperience>;

export const Populated: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
};

export const Empty: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const AddExperience: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const add = canvasElement.querySelector<HTMLElement>('[data-testid="add-experience"]');
    if (add === null) {
      throw new Error('Add experience story could not find the Add experience action.');
    }
    add.click();
    await settle();
  },
};

export const EditExperience: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const edit = canvasElement.querySelector<HTMLElement>('[data-testid="experience-edit"]');
    if (edit === null) {
      throw new Error('Edit experience story could not find an Edit action.');
    }
    edit.click();
    await settle();
  },
};

export const DeleteConfirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new PopulatedApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const remove = canvasElement.querySelector<HTMLElement>('[data-testid="experience-delete"]');
    if (remove === null) {
      throw new Error('Delete confirmation story could not find a Delete action.');
    }
    remove.click();
    await settle();
  },
};
