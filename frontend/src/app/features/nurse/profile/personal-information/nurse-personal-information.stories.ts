import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NursePersonalInformation } from './nurse-personal-information';

const PROFILE = {
  id: 'profile-1',
  userId: 'user-1',
  headline: 'ICU Registered Nurse',
  professionalSummary: 'Critical care nurse with eight years of experience in high-acuity units.',
  licenseNumber: 'RN-12345',
  licenseCountryId: 'country-sa',
  licenseCountryName: 'Saudi Arabia',
  currentCountryId: 'country-sa',
  currentCountryName: 'Saudi Arabia',
  yearsOfExperience: 8,
  isAvailableForRecruitment: true,
};

const COUNTRIES = [
  { id: 'country-au', name: 'Australia', code: 'AU' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-us', name: 'United States', code: 'US' },
];

class EditApi {
  getProfile() {
    return of(PROFILE);
  }
  upsertProfile() {
    return of(PROFILE);
  }
}

class CreateApi {
  getProfile() {
    return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
  }
  upsertProfile() {
    return of({ ...PROFILE, headline: null });
  }
}

class CountriesApiStub {
  list() {
    return of(COUNTRIES);
  }
}

const meta: Meta<NursePersonalInformation> = {
  component: NursePersonalInformation,
  title: 'Features/Nurse/PersonalInformation',
};
export default meta;
type Story = StoryObj<NursePersonalInformation>;

export const EditPrefilled: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: [
        provideRouter([]),
        { provide: NurseProfileApi, useValue: new EditApi() },
        { provide: CountriesApi, useValue: new CountriesApiStub() },
      ],
    }),
  ],
};

export const Create: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: [
        provideRouter([]),
        { provide: NurseProfileApi, useValue: new CreateApi() },
        { provide: CountriesApi, useValue: new CountriesApiStub() },
      ],
    }),
  ],
};
