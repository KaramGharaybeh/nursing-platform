import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseProfileDto } from '../../../../core/api/generated/models/nurse-profile-dto';
import { routes } from '../../../../app.routes';
import { NursePersonalInformation } from './nurse-personal-information';

const PROFILE: NurseProfileDto = {
  id: 'profile-1',
  userId: 'user-1',
  headline: 'ICU Registered Nurse',
  professionalSummary: 'Critical care nurse.',
  licenseNumber: 'RN-12345',
  licenseCountryId: 'country-sa',
  licenseCountryName: 'Saudi Arabia',
  currentCountryId: 'country-sa',
  currentCountryName: 'Saudi Arabia',
  yearsOfExperience: 7,
  isAvailableForRecruitment: true,
};

const COUNTRIES = [
  { id: 'country-ca', name: 'Canada', code: 'CA' },
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
];

class NurseProfileApiStub {
  readonly upsertCalls: unknown[] = [];
  nextProfile: unknown = PROFILE;
  nextProfileError: unknown = undefined;
  nextUpsertError: unknown = undefined;
  getProfile() {
    if (this.nextProfileError !== undefined) {
      return throwError(() => this.nextProfileError);
    }
    return of(this.nextProfile as NurseProfileDto);
  }
  upsertProfile(request: unknown) {
    this.upsertCalls.push(request);
    if (this.nextUpsertError !== undefined) {
      return throwError(() => this.nextUpsertError);
    }
    return of({ ...(this.nextProfile as NurseProfileDto), ...(request as object) });
  }
}

class CountriesApiStub {
  list() {
    return of(COUNTRIES);
  }
}

async function setup(
  api = new NurseProfileApiStub(),
  countriesApi: CountriesApiStub = new CountriesApiStub(),
) {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NursePersonalInformation],
    providers: [
      provideRouter([]),
      { provide: NurseProfileApi, useValue: api },
      { provide: CountriesApi, useValue: countriesApi },
      { provide: ActivatedRoute, useValue: { snapshot: {} } },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NursePersonalInformation);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 50));
  fixture.detectChanges();
  return { fixture, api };
}


interface PersonalInformationFormLike {
  controls: {
    headline: { value: string };
    professionalSummary: { value: string };
    licenseNumber: { value: string };
    licenseCountryId: { value: string };
    currentCountryId: { value: string };
    yearsOfExperience: { value: number };
    isAvailableForRecruitment: { value: boolean };
  };
}

describe('NursePersonalInformation', () => {
  it('renders exactly the seven supported fields with approved grouping', async () => {
    const { fixture } = await setup();
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Edit personal information');
    expect(text).toContain('Professional identity');
    expect(text).toContain('License and location');
    expect(text).toContain('Experience');
    expect(text).toContain('Recruitment');
    expect(text).toContain('Headline');
    expect(text).toContain('Professional summary');
    expect(text).toContain('License number');
    expect(text).toContain('License country');
    expect(text).toContain('Current country');
    expect(text).toContain('Years of experience');
    expect(text).toContain('Available for recruitment');
    expect(text).not.toContain('phone');
    expect(text).not.toContain('Photo');
    expect(text).not.toContain('specialty');
  });

  it('edit mode prefills all seven controls from the loaded profile', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as { form: PersonalInformationFormLike; countryOptions: unknown; };

    expect(component.form.controls.headline.value).toBe('ICU Registered Nurse');
    expect(component.form.controls.professionalSummary.value).toBe('Critical care nurse.');
    expect(component.form.controls.licenseNumber.value).toBe('RN-12345');
    expect(component.form.controls.licenseCountryId.value).toBe('country-sa');
    expect(component.form.controls.currentCountryId.value).toBe('country-sa');
    expect(component.form.controls.yearsOfExperience.value).toBe(7);
    expect(component.form.controls.isAvailableForRecruitment.value).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Edit personal information');
  });

  it('create mode initializes defaults when the profile GET returns 404', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as { form: PersonalInformationFormLike; countryOptions: unknown; };

    expect(fixture.nativeElement.textContent).toContain('Add personal information');
    expect(component.form.controls.yearsOfExperience.value).toBe(0);
    expect(component.form.controls.isAvailableForRecruitment.value).toBe(false);
  });

  it('submits the complete form state through the upsert contract and navigates to the overview', async () => {
    const { fixture, api } = await setup();
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await (fixture.componentInstance as unknown as { submit: () => Promise<void> }).submit();

    expect(api.upsertCalls).toEqual([
      {
        headline: 'ICU Registered Nurse',
        professionalSummary: 'Critical care nurse.',
        licenseNumber: 'RN-12345',
        licenseCountryId: 'country-sa',
        currentCountryId: 'country-sa',
        yearsOfExperience: 7,
        isAvailableForRecruitment: true,
      },
    ]);
    expect(navigateSpy).toHaveBeenCalledWith('/nurse/profile');
  });

  it('trims empty optional values to null on submit', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);

    await (fixture.componentInstance as unknown as { submit: () => Promise<void> }).submit();

    expect(api.upsertCalls).toEqual([
      {
        headline: null,
        professionalSummary: null,
        licenseNumber: null,
        licenseCountryId: null,
        currentCountryId: null,
        yearsOfExperience: 0,
        isAvailableForRecruitment: false,
      },
    ]);
  });

  it('rejects out-of-range years of experience with client validation mirroring the backend', async () => {
    const { fixture, api } = await setup();
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    (fixture.componentInstance as unknown as { updateYearsOfExperience: (v: string) => void }).updateYearsOfExperience('81');
    await (fixture.componentInstance as unknown as { submit: () => Promise<void> }).submit();
    fixture.detectChanges();

    expect(api.upsertCalls).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('Years of experience must be between 0 and 80.');
  });

  it('keeps the user on the page with a problem-details message when the backend rejects the save', async () => {
    const api = new NurseProfileApiStub();
    api.nextUpsertError = { status: 409, error: { title: 'Conflict', status: 409, detail: 'One or more countries are invalid or inactive.' } };
    const { fixture } = await setup(api);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await (fixture.componentInstance as unknown as { submit: () => Promise<void> }).submit();
    fixture.detectChanges();

    expect(navigateSpy).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('One or more countries are invalid or inactive.');
  });

  it('renders an error and retry affordance when loading fails generically', async () => {
    const api = new NurseProfileApiStub();
    api.nextProfileError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Server Error');
    expect(text).toContain('Try again');
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });

  it('populates country selects from the Country lookup and not from hardcoded data', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as { form: PersonalInformationFormLike; countryOptions: unknown; };

    expect(component.countryOptions).toEqual([
      { value: 'country-ca', label: 'Canada' },
      { value: 'country-sa', label: 'Saudi Arabia' },
    ]);
  });
});

describe('Nurse personal information route', () => {
  it('mounts /nurse/profile/personal-information with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/personal-information');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_PERSONAL_INFORMATION' });
  });
});
