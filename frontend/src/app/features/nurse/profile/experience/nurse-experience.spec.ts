import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NurseExperienceDto } from '../../../../core/api/generated/models/nurse-experience-dto';
import { NurseExperience } from './nurse-experience';
import type { NurseExperienceFormValue } from './nurse-experience-form';

const COUNTRIES: readonly CountryListItemDto[] = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const FIRST: NurseExperienceDto = {
  id: 'exp-1',
  facilityName: 'City General Hospital',
  jobTitle: 'Emergency Department Nurse',
  countryId: 'country-sa',
  countryName: 'Saudi Arabia',
  startDate: '2019-04-01',
  endDate: null,
  isCurrent: true,
  description: 'Triage and resuscitation across a busy urban emergency department with high-acuity intake, rapid assessment, and coordination with physicians and specialists around the clock.',
};

const SECOND: NurseExperienceDto = {
  id: 'exp-2',
  facilityName: 'Riverside Clinic',
  jobTitle: 'Staff Nurse',
  countryId: null,
  countryName: null,
  startDate: '2016-01-15',
  endDate: '2019-03-30',
  isCurrent: false,
  description: null,
};

class ExperienceApiStub {
  experiences: NurseExperienceDto[] = [FIRST, SECOND];
  listError: unknown = undefined;
  updateError: unknown = undefined;
  deleteError: unknown = undefined;
  created: { body: unknown }[] = [];
  updated: { id: string; body: unknown }[] = [];
  deleted: string[] = [];

  listExperiences() {
    return this.listError === undefined ? of(this.experiences) : throwError(() => this.listError);
  }

  createExperience(body: unknown) {
    this.created.push({ body });
    const created: NurseExperienceDto = {
      id: 'exp-new',
      facilityName: 'New Hospital',
      jobTitle: 'ICU Nurse',
      countryId: 'country-ca',
      countryName: 'Canada',
      startDate: '2024-02-01',
      endDate: null,
      isCurrent: true,
      description: null,
    };
    this.experiences = [created, ...this.experiences];
    return of(created);
  }

  updateExperience(id: string, body: unknown) {
    this.updated.push({ id, body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    return of({ ...FIRST, id });
  }

  deleteExperience(id: string) {
    this.deleted.push(id);
    if (this.deleteError !== undefined) {
      return throwError(() => this.deleteError);
    }
    this.experiences = this.experiences.filter((experience) => experience.id !== id);
    return of(undefined);
  }
}

class CountriesApiStub {
  list() {
    return of(COUNTRIES);
  }
}

async function setup(api?: ExperienceApiStub): Promise<{ fixture: ComponentFixture<NurseExperience>; api: ExperienceApiStub; announcer: Announcer }> {
  const stub = api ?? new ExperienceApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseExperience],
    providers: [
      provideRouter([]),
      { provide: NurseProfileApi, useValue: stub },
      { provide: CountriesApi, useValue: new CountriesApiStub() },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseExperience);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseExperience>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<NurseExperience>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

function allByTestId(fixture: ComponentFixture<NurseExperience>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseExperience>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseExperience', () => {
  it('renders records in backend order with supported fields only', async () => {
    const { fixture } = await setup();
    const content = text(fixture);
    const cards = allByTestId(fixture, 'experience-card');

    expect(cards.length).toBe(2);
    expect(cards[0].textContent).toContain('Emergency Department Nurse');
    expect(cards[1].textContent).toContain('Staff Nurse');
    expect(cards[0].textContent).toContain('Apr 1, 2019');
    expect(cards[1].textContent).toContain('Mar 30, 2019');
    expect(content).toContain('City General Hospital');
    expect(content).toContain('Saudi Arabia');
    expect(content).toContain('Present');
    expect(content).not.toContain('Employment type');
    expect(content).not.toContain('Salary');
    expect(content).not.toContain('Specialty');
  });

  it('marks the current role with a Current badge', async () => {
    const { fixture } = await setup();
    const cards = allByTestId(fixture, 'experience-card');

    expect(cards[0].textContent).toContain('Current');
    expect(cards[1].textContent).not.toContain('Current');
  });

  it('shows a calm empty state with Add experience switching to create mode on the same route', async () => {
    const api = new ExperienceApiStub();
    api.experiences = [];
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No positions added yet.');
    byTestId(fixture, 'add-experience')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(text(fixture)).toContain('Add experience');
    expect(fixture.nativeElement.querySelector('np-nurse-experience-form')).not.toBeNull();
  });

  it('creates a record with the exact request body then returns to the refreshed list', async () => {
    const { fixture, api, announcer } = await setup();
    byTestId(fixture, 'add-experience')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-experience-form',
    ).componentInstance as {
      save: { emit(value: NurseExperienceFormValue): void };
    };
    form.save.emit({
      facilityName: 'New Hospital',
      jobTitle: 'ICU Nurse',
      countryId: 'country-ca',
      startDate: '2024-02-01',
      endDate: null,
      isCurrent: true,
      description: null,
    });
    await settle(fixture);

    expect(api.created.length).toBe(1);
    expect(api.created[0].body).toEqual({
      facilityName: 'New Hospital',
      jobTitle: 'ICU Nurse',
      countryId: 'country-ca',
      startDate: '2024-02-01',
      endDate: null,
      isCurrent: true,
      description: null,
    });
    expect(text(fixture)).toContain('ICU Nurse');
    expect(announcer.current()?.text).toBe('Experience saved.');
  });

  it('cancels create mode without calling the backend', async () => {
    const { fixture, api } = await setup();
    byTestId(fixture, 'add-experience')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-experience-form',
    ).componentInstance as { cancelled: { emit(): void } };
    form.cancelled.emit();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.created.length).toBe(0);
    expect(allByTestId(fixture, 'experience-card').length).toBe(2);
  });

  it('prefills the edit form from the loaded record and updates by id', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'experience-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-experience-form',
    ).componentInstance as {
      initial: NurseExperienceFormValue | undefined;
      save: { emit(value: NurseExperienceFormValue): void };
    };
    expect(form.initial).toEqual({
      facilityName: 'City General Hospital',
      jobTitle: 'Emergency Department Nurse',
      countryId: 'country-sa',
      startDate: '2019-04-01',
      endDate: null,
      isCurrent: true,
      description: 'Triage and resuscitation across a busy urban emergency department with high-acuity intake, rapid assessment, and coordination with physicians and specialists around the clock.',
    });

    form.save.emit({ ...(form.initial as NurseExperienceFormValue), jobTitle: 'Senior Emergency Nurse' });
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].id).toBe('exp-1');
    expect((api.updated[0].body as { jobTitle: string }).jobTitle).toBe('Senior Emergency Nurse');
    expect(fixture.nativeElement.querySelector('np-nurse-experience-form')).toBeNull();
  });

  it('shows a record-missing message when update returns 404', async () => {
    const api = new ExperienceApiStub();
    api.updateError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'experience-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-experience-form',
    ).componentInstance as {
      save: { emit(value: NurseExperienceFormValue): void };
    };
    form.save.emit({
      facilityName: 'City General Hospital',
      jobTitle: 'Emergency Department Nurse',
      countryId: 'country-sa',
      startDate: '2019-04-01',
      endDate: null,
      isCurrent: true,
      description: null,
    });
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(text(fixture)).toContain('no longer exists');
  });

  it('requires a second explicit action before deleting and never deletes on first click', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'experience-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(text(fixture)).toContain('This cannot be undone.');
    expect(byTestId(fixture, 'experience-confirm-delete')).not.toBeNull();
  });

  it('cancels the inline confirmation with Keep without mutating', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'experience-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'experience-keep')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(byTestId(fixture, 'experience-confirm-delete')).toBeNull();
    expect(allByTestId(fixture, 'experience-card').length).toBe(2);
  });

  it('deletes on confirm, removes the card, and announces the outcome', async () => {
    const { fixture, api, announcer } = await setup();
    allByTestId(fixture, 'experience-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'experience-confirm-delete')?.click();
    await settle(fixture);

    expect(api.deleted).toEqual(['exp-1']);
    expect(allByTestId(fixture, 'experience-card').length).toBe(1);
    expect(announcer.current()?.text).toBe('Experience deleted.');
  });

  it('treats delete 404 as already removed with a calm notice', async () => {
    const api = new ExperienceApiStub();
    api.deleteError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture, announcer } = await setup(api);
    allByTestId(fixture, 'experience-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'experience-confirm-delete')?.click();
    await settle(fixture);

    expect(text(fixture)).toContain('already been removed');
    expect(announcer.current()?.text).toBe('Experience was already removed.');
  });

  it('shows an inline retryable error when delete fails generically', async () => {
    const api = new ExperienceApiStub();
    api.deleteError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'experience-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'experience-confirm-delete')?.click();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(allByTestId(fixture, 'experience-card').length).toBe(2);
    expect(text(fixture)).toContain('could not be deleted');
  });

  it('renders a loading state and a retry affordance on load failure', async () => {
    const api = new ExperienceApiStub();
    api.listError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
    expect(allByTestId(fixture, 'experience-card').length).toBe(0);
  });

  it('toggles long descriptions with Show more and Show less', async () => {
    const { fixture } = await setup();

    expect(allByTestId(fixture, 'experience-toggle-description').length).toBe(1);
    byTestId(fixture, 'experience-toggle-description')?.click();
    fixture.detectChanges();

    expect(text(fixture)).toContain('Show less');
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse experience route', () => {
  it('mounts /nurse/profile/experience with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/experience');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_EXPERIENCE' });
  });
});
