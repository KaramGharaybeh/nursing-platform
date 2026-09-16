import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { CurrentUserStore } from '../../../../core/auth/current-user-store';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseProfileOverview } from './nurse-profile-overview';

const PROFILE = {
  id: 'profile-1',
  userId: 'user-1',
  headline: 'ICU Registered Nurse',
  professionalSummary: 'Critical care nurse with eight years of experience in high-acuity units.',
  licenseNumber: 'RN-12345',
  licenseCountryId: 'country-1',
  licenseCountryName: 'Saudi Arabia',
  currentCountryId: 'country-1',
  currentCountryName: 'Saudi Arabia',
  yearsOfExperience: 8,
  isAvailableForRecruitment: true,
};

class PopulatedApi {
  getProfile() {
    return of(PROFILE);
  }
  listExperiences() {
    return of([
      {
        id: 'exp-1',
        facilityName: 'General Hospital',
        jobTitle: 'Staff Nurse',
        startDate: '2024-01-01',
        endDate: null,
        isCurrent: true,
        description: null,
        countryId: null,
        countryName: null,
      },
    ]);
  }
  listEducation() {
    return of([
      {
        id: 'edu-1',
        institutionName: 'University of Nursing',
        degree: 'Bachelor of Nursing',
        fieldOfStudy: 'Nursing',
        startDate: null,
        endDate: null,
        description: null,
        countryId: null,
        countryName: null,
      },
    ]);
  }
  listCertificates() {
    return of([
      { id: 'cert-1', name: 'BLS', issuingOrganization: 'AHA', issueDate: null, expirationDate: null, credentialId: null, credentialUrl: null },
    ]);
  }
  listSkills() {
    return of([
      { id: 'skill-1', name: 'Triage' },
      { id: 'skill-2', name: 'Critical Care' },
      { id: 'skill-3', name: 'Patient Education' },
    ]);
  }
  listLanguages() {
    return of([
      { id: 'lang-1', languageId: 'l1', name: 'Arabic', code: 'AR', proficiency: 'Native' },
      { id: 'lang-2', languageId: 'l2', name: 'English', code: 'EN', proficiency: 'Fluent' },
    ]);
  }
  getCv() {
    return of({
      id: 'cv-1',
      fileName: 'nora-cv.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 153600,
      uploadedAt: '2026-09-16T10:00:00Z',
    });
  }
}

class FirstTimeApi {
  getProfile() {
    return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
  }
  listExperiences() {
    return of([]);
  }
  listEducation() {
    return of([]);
  }
  listCertificates() {
    return of([]);
  }
  listSkills() {
    return of([]);
  }
  listLanguages() {
    return of([]);
  }
  getCv() {
    return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
  }
}

class CurrentUserStoreStub {
  currentUser = () => ({ firstName: 'Nora', lastName: 'Nurse', username: 'noranurse' });
}

const meta: Meta<NurseProfileOverview> = {
  component: NurseProfileOverview,
  title: 'Features/Nurse/ProfileOverview',
  decorators: [
    (story) => ({
      ...story(),
      providers: [provideRouter([])],
    }),
  ],
};
export default meta;
type Story = StoryObj<NurseProfileOverview>;

export const Populated: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: [
        provideRouter([]),
        { provide: NurseProfileApi, useValue: new PopulatedApi() },
        { provide: CurrentUserStore, useValue: new CurrentUserStoreStub() },
      ],
    }),
  ],
};

export const FirstTime: Story = {
  decorators: [
    (story) => ({
      ...story(),
      providers: [
        provideRouter([]),
        { provide: NurseProfileApi, useValue: new FirstTimeApi() },
        { provide: CurrentUserStore, useValue: new CurrentUserStoreStub() },
      ],
    }),
  ],
};
