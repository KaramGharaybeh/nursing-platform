import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CurrentUserStore } from '../../../../core/auth/current-user-store';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseProfileDto } from '../../../../core/api/generated/models/nurse-profile-dto';
import { routes } from '../../../../app.routes';
import { NurseProfileOverview } from './nurse-profile-overview';

const PROFILE: NurseProfileDto = {
  id: 'profile-1',
  userId: 'user-1',
  headline: 'ICU Registered Nurse',
  professionalSummary: 'Critical care nurse with a calm approach.',
  licenseNumber: 'RN-12345',
  licenseCountryId: 'country-1',
  licenseCountryName: 'Saudi Arabia',
  currentCountryId: 'country-1',
  currentCountryName: 'Saudi Arabia',
  yearsOfExperience: 7,
  isAvailableForRecruitment: true,
};

class NurseProfileApiStub {
  nextProfile: unknown = PROFILE;
  nextProfileError: unknown = undefined;
  readonly experienceCalls = 0;
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
      {
        id: 'exp-2',
        facilityName: 'City Clinic',
        jobTitle: 'Clinic Nurse',
        startDate: '2022-01-01',
        endDate: '2023-12-31',
        isCurrent: false,
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
        fieldOfStudy: null,
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
      { id: 'cert-2', name: 'ACLS', issuingOrganization: 'AHA', issueDate: null, expirationDate: null, credentialId: null, credentialUrl: null },
      { id: 'cert-3', name: 'PALS', issuingOrganization: 'AHA', issueDate: null, expirationDate: null, credentialId: null, credentialUrl: null },
    ]);
  }
  listSkills() {
    return of([
      { id: 'skill-1', name: 'Triage' },
      { id: 'skill-2', name: 'Critical Care' },
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
      fileName: 'cv.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 2048,
      uploadedAt: '2026-09-16T10:00:00Z',
    });
  }
  getProfile() {
    if (this.nextProfileError !== undefined) {
      return throwError(() => this.nextProfileError);
    }
    return of(this.nextProfile as NurseProfileDto);
  }
}

class CurrentUserStoreStub {
  currentUser = () => ({ firstName: 'Nora', lastName: 'Nurse', username: 'noranurse' });
}

async function setup(api = new NurseProfileApiStub()) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseProfileOverview],
    providers: [
      provideRouter([]),
      { provide: NurseProfileApi, useValue: api },
      { provide: CurrentUserStore, useValue: new CurrentUserStoreStub() },
      { provide: ActivatedRoute, useValue: { snapshot: {} } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseProfileOverview);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 50));
  fixture.detectChanges();
  return { fixture, api };
}

describe('NurseProfileOverview', () => {
  it('loads and renders the professional identity area from the supported fields', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Nora Nurse');
    expect(text).toContain('ICU Registered Nurse');
    expect(text).toContain('Critical care nurse with a calm approach.');
    expect(text).toContain('License number: RN-12345');
    expect(text).toContain('Years of experience');
    expect(text).toContain('7');
    expect(text).toContain('Saudi Arabia');
  });

  it('renders availability using only the approved binary labels', async () => {
    const { fixture } = await setup();
    expect(fixture.nativeElement.textContent).toContain('Available for recruitment');

    const api = new NurseProfileApiStub();
    api.nextProfile = { ...PROFILE, isAvailableForRecruitment: false };
    const { fixture: unavailable } = await setup(api);
    expect(unavailable.nativeElement.textContent).toContain('Not available for recruitment');
  });

  it('renders section summaries with approved depth only', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Experience');
    expect(text).toContain('Staff Nurse');
    expect(text).toContain('+ 1 more positions');
    expect(text).toContain('Bachelor of Nursing');
    expect(text).toContain('BLS');
    expect(text).toContain('ACLS');
    expect(text).toContain('+ 1 more certificates');
    expect(text).not.toContain('PALS');
    expect(text).toContain('Triage');
    expect(text).toContain('Arabic · Native');
    expect(text).toContain('English · Fluent');
    expect(text).toContain('cv.pdf');
    expect(text).toContain('uploaded');
  });

  it('renders the calm first-time state when the profile GET returns 404', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('No professional Nurse Profile has been created yet.');
    expect(text).not.toContain('could not be loaded');
    expect(text).not.toContain('Staff Nurse');
  });

  it('renders an error and retry affordance on generic load failure', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Server Error');
    expect(text).toContain('Try again');
    expect(text).not.toContain('No professional Nurse Profile has been created yet');
  });

  it('exposes all seven section actions and no completion UI', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;
    const anchors = Array.from(fixture.nativeElement.querySelectorAll('a') as NodeListOf<HTMLAnchorElement>);

    expect(anchors.length).toBe(7);
    expect(anchors[0].textContent?.trim()).toBe('Edit personal information');
    expect(anchors[1].textContent?.trim()).toBe('Manage experience');
    expect(anchors[2].textContent?.trim()).toBe('Manage education');
    expect(anchors[3].textContent?.trim()).toBe('Manage certificates');
    expect(anchors[4].textContent?.trim()).toBe('Manage skills');
    expect(anchors[4].getAttribute('href')).toBe('/nurse/profile/skills');
    expect(anchors[5].textContent?.trim()).toBe('Manage languages');
    expect(anchors[5].getAttribute('href')).toBe('/nurse/profile/languages');
    expect(anchors[6].textContent?.trim()).toBe('Manage CV');
    expect(anchors[6].getAttribute('href')).toBe('/nurse/profile/cv');
    expect(text).not.toContain('%');
    expect(text).not.toContain('%');
    expect(text).not.toContain('completion');
    expect(text).not.toContain('profile strength');
    expect(text).not.toContain('Active Career Profile');
    expect(text).not.toContain('verified');
  });

  it('exposes the approved first-time CTA to personal information in the empty state', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Add personal information');
    const anchors = fixture.nativeElement.querySelectorAll('a') as NodeListOf<HTMLAnchorElement>;
    expect(anchors.length).toBe(0);
  });
});

describe('Nurse routes', () => {
  it('mounts /nurse as a redirect to /nurse/profile', () => {
    const nurseEntry = routes.find((route) => route.path === 'nurse');

    expect(nurseEntry).toBeDefined();
    expect(nurseEntry?.redirectTo).toBe('nurse/profile');
    expect(nurseEntry?.pathMatch).toBe('full');
    expect(nurseEntry?.loadComponent).toBeUndefined();
  });

  it('mounts /nurse/profile with the three-guard pattern and NURSE_PROFILE_OVERVIEW routeId', async () => {
    const overviewRoute = routes.find((route) => route.path === 'nurse/profile');

    expect(overviewRoute).toBeDefined();
    expect(typeof overviewRoute?.loadComponent).toBe('function');
    expect(overviewRoute?.canActivate).toBeDefined();
    expect(overviewRoute?.canActivate?.length).toBe(3);
    expect(overviewRoute?.data).toEqual({ routeId: 'NURSE_PROFILE_OVERVIEW' });
  });
});
