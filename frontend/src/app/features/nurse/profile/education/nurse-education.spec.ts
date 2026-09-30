import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { CountriesApi } from '../../../../core/api/countries-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import type { NurseEducationDto } from '../../../../core/api/generated/models/nurse-education-dto';
import { NurseEducation } from './nurse-education';
import type { NurseEducationFormValue } from './nurse-education-form';

const COUNTRIES: readonly CountryListItemDto[] = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const ONGOING: NurseEducationDto = {
  id: 'edu-1',
  institutionName: 'Harbor Institute',
  degree: 'Master of Nursing',
  fieldOfStudy: 'Nursing Education',
  countryId: null,
  countryName: null,
  startDate: '2023-09-01',
  endDate: null,
  description: 'Advanced study in clinical teaching and curriculum design for nursing programs, with supervised practicum hours, simulation lab work, and a capstone research project on competency assessment.',
};

const COMPLETED: NurseEducationDto = {
  id: 'edu-2',
  institutionName: 'Riyadh College of Nursing',
  degree: 'Bachelor of Science in Nursing',
  fieldOfStudy: null,
  countryId: 'country-sa',
  countryName: 'Saudi Arabia',
  startDate: '2015-09-01',
  endDate: '2019-06-30',
  description: null,
};

const DATELESS: NurseEducationDto = {
  id: 'edu-3',
  institutionName: 'Harbor Institute',
  degree: 'Diploma in Wound Care',
  fieldOfStudy: null,
  countryId: null,
  countryName: null,
  startDate: null,
  endDate: null,
  description: null,
};

class EducationApiStub {
  records: NurseEducationDto[] = [ONGOING, COMPLETED, DATELESS];
  listError: unknown = undefined;
  updateError: unknown = undefined;
  deleteError: unknown = undefined;
  created: { body: unknown }[] = [];
  updated: { id: string; body: unknown }[] = [];
  deleted: string[] = [];

  listEducation() {
    return this.listError === undefined ? of(this.records) : throwError(() => this.listError);
  }

  createEducation(body: unknown) {
    this.created.push({ body });
    const created: NurseEducationDto = {
      id: 'edu-new',
      institutionName: 'North University',
      degree: 'Bachelor of Science in Nursing',
      fieldOfStudy: 'Pediatrics',
      countryId: 'country-ca',
      countryName: 'Canada',
      startDate: '2016-09-01',
      endDate: '2020-06-30',
      description: null,
    };
    this.records = [created, ...this.records];
    return of(created);
  }

  updateEducation(id: string, body: unknown) {
    this.updated.push({ id, body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    return of({ ...ONGOING, id });
  }

  deleteEducation(id: string) {
    this.deleted.push(id);
    if (this.deleteError !== undefined) {
      return throwError(() => this.deleteError);
    }
    this.records = this.records.filter((record) => record.id !== id);
    return of(undefined);
  }
}

class CountriesApiStub {
  list() {
    return of(COUNTRIES);
  }
}

async function setup(api?: EducationApiStub): Promise<{ fixture: ComponentFixture<NurseEducation>; api: EducationApiStub; announcer: Announcer }> {
  const stub = api ?? new EducationApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseEducation],
    providers: [
      provideRouter([]),
      { provide: NurseProfileApi, useValue: stub },
      { provide: CountriesApi, useValue: new CountriesApiStub() },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseEducation);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseEducation>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<NurseEducation>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

function allByTestId(fixture: ComponentFixture<NurseEducation>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseEducation>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseEducation', () => {
  it('renders records in backend order with supported fields only', async () => {
    const { fixture } = await setup();
    const content = text(fixture);
    const cards = allByTestId(fixture, 'education-card');

    expect(cards.length).toBe(3);
    expect(cards[0].textContent).toContain('Master of Nursing');
    expect(cards[1].textContent).toContain('Bachelor of Science in Nursing');
    expect(cards[2].textContent).toContain('Diploma in Wound Care');
    expect(content).toContain('Riyadh College of Nursing');
    expect(content).toContain('Saudi Arabia');
    expect(content).toContain('Nursing Education');
    expect(content).not.toContain('GPA');
    expect(content).not.toContain('Honors');
    expect(content).not.toContain('Accreditation');
    expect(content).not.toContain('Transcript');
  });

  it('renders date lines per HD-E1 without inventing missing-date warnings', async () => {
    const { fixture } = await setup();
    const cards = allByTestId(fixture, 'education-card');

    expect(cards[0].textContent).toContain('Sep 1, 2023 → Present');
    expect(cards[1].textContent).toContain('Sep 1, 2015');
    expect(cards[1].textContent).toContain('Jun 30, 2019');
    expect(cards[2].querySelector('[data-testid="education-dates"]')).toBeNull();
    expect(text(fixture)).not.toContain('Dates not provided');
  });

  it('shows a calm empty state with Add education switching to create mode on the same route', async () => {
    const api = new EducationApiStub();
    api.records = [];
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No education added yet.');
    byTestId(fixture, 'add-education')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(text(fixture)).toContain('Add education');
    expect(fixture.nativeElement.querySelector('np-nurse-education-form')).not.toBeNull();
  });

  it('creates a record with the exact request body then returns to the refreshed list', async () => {
    const { fixture, api, announcer } = await setup();
    byTestId(fixture, 'add-education')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-education-form',
    ).componentInstance as {
      save: { emit(value: NurseEducationFormValue): void };
    };
    form.save.emit({
      institutionName: 'North University',
      degree: 'Bachelor of Science in Nursing',
      fieldOfStudy: 'Pediatrics',
      countryId: 'country-ca',
      startDate: '2016-09-01',
      endDate: '2020-06-30',
      description: null,
    });
    await settle(fixture);

    expect(api.created.length).toBe(1);
    expect(api.created[0].body).toEqual({
      institutionName: 'North University',
      degree: 'Bachelor of Science in Nursing',
      fieldOfStudy: 'Pediatrics',
      countryId: 'country-ca',
      startDate: '2016-09-01',
      endDate: '2020-06-30',
      description: null,
    });
    expect(text(fixture)).toContain('North University');
    expect(announcer.current()?.text).toBe('Education saved.');
  });

  it('cancels create mode without calling the backend', async () => {
    const { fixture, api } = await setup();
    byTestId(fixture, 'add-education')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-education-form',
    ).componentInstance as { cancelled: { emit(): void } };
    form.cancelled.emit();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.created.length).toBe(0);
    expect(allByTestId(fixture, 'education-card').length).toBe(3);
  });

  it('prefills the edit form from the loaded record and updates by id', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'education-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-education-form',
    ).componentInstance as {
      initial: NurseEducationFormValue | undefined;
      save: { emit(value: NurseEducationFormValue): void };
    };
    expect(form.initial).toEqual({
      institutionName: 'Harbor Institute',
      degree: 'Master of Nursing',
      fieldOfStudy: 'Nursing Education',
      countryId: null,
      startDate: '2023-09-01',
      endDate: null,
      description: 'Advanced study in clinical teaching and curriculum design for nursing programs, with supervised practicum hours, simulation lab work, and a capstone research project on competency assessment.',
    });

    form.save.emit({ ...(form.initial as NurseEducationFormValue), degree: 'Doctor of Nursing' });
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].id).toBe('edu-1');
    expect((api.updated[0].body as { degree: string }).degree).toBe('Doctor of Nursing');
    expect(fixture.nativeElement.querySelector('np-nurse-education-form')).toBeNull();
  });

  it('shows a record-missing message when update returns 404', async () => {
    const api = new EducationApiStub();
    api.updateError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'education-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-education-form',
    ).componentInstance as {
      save: { emit(value: NurseEducationFormValue): void };
    };
    form.save.emit({
      institutionName: 'Harbor Institute',
      degree: 'Master of Nursing',
      fieldOfStudy: null,
      countryId: null,
      startDate: null,
      endDate: null,
      description: null,
    });
    await settle(fixture);

    expect(text(fixture)).toContain('no longer exists');
  });

  it('requires a second explicit action before deleting and never deletes on first click', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'education-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(text(fixture)).toContain('This cannot be undone.');
    expect(byTestId(fixture, 'education-confirm-delete')).not.toBeNull();
  });

  it('cancels the inline confirmation with Keep without mutating', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'education-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'education-keep')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(byTestId(fixture, 'education-confirm-delete')).toBeNull();
    expect(allByTestId(fixture, 'education-card').length).toBe(3);
  });

  it('deletes on confirm, removes the card, and announces the outcome', async () => {
    const { fixture, api, announcer } = await setup();
    allByTestId(fixture, 'education-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'education-confirm-delete')?.click();
    await settle(fixture);

    expect(api.deleted).toEqual(['edu-1']);
    expect(allByTestId(fixture, 'education-card').length).toBe(2);
    expect(announcer.current()?.text).toBe('Education deleted.');
  });

  it('treats delete 404 as already removed with a calm notice', async () => {
    const api = new EducationApiStub();
    api.deleteError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture, announcer } = await setup(api);
    allByTestId(fixture, 'education-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'education-confirm-delete')?.click();
    await settle(fixture);

    expect(text(fixture)).toContain('already been removed');
    expect(announcer.current()?.text).toBe('Education was already removed.');
  });

  it('shows an inline retryable error when delete fails generically', async () => {
    const api = new EducationApiStub();
    api.deleteError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'education-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'education-confirm-delete')?.click();
    await settle(fixture);

    expect(allByTestId(fixture, 'education-card').length).toBe(3);
    expect(text(fixture)).toContain('could not be deleted');
  });

  it('renders an error and retry affordance on load failure', async () => {
    const api = new EducationApiStub();
    api.listError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
    expect(allByTestId(fixture, 'education-card').length).toBe(0);
  });

  it('toggles long descriptions with Show more and Show less', async () => {
    const { fixture } = await setup();

    expect(allByTestId(fixture, 'education-toggle-description').length).toBe(1);
    byTestId(fixture, 'education-toggle-description')?.click();
    fixture.detectChanges();

    expect(text(fixture)).toContain('Show less');
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse education route', () => {
  it('mounts /nurse/profile/education with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/education');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_EDUCATION' });
  });
});
