import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import {
  NurseExperienceForm,
  type NurseExperienceFormValue,
} from './nurse-experience-form';

interface ExperienceFormLike {
  form: {
    controls: Record<
      'facilityName' | 'jobTitle' | 'countryId' | 'startDate' | 'endDate' | 'description' | 'isCurrent',
      { value: string | boolean; disabled: boolean; enabled: boolean }
    >;
  };
  updateJobTitle(value: string): void;
  updateFacilityName(value: string): void;
  updateCountry(value: string): void;
  updateStartDate(value: string): void;
  updateEndDate(value: string): void;
  updateDescription(value: string): void;
  updateIsCurrent(checked: boolean): void;
  submit(): Promise<void>;
  cancel(): void;
  save: { subscribe(next: (value: NurseExperienceFormValue) => void): void };
  cancelled: { subscribe(next: () => void): void };
}

function formOf(fixture: ComponentFixture<NurseExperienceForm>): ExperienceFormLike {
  return fixture.componentInstance as unknown as ExperienceFormLike;
}

const COUNTRIES: readonly CountryListItemDto[] = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const EDIT_VALUE: NurseExperienceFormValue = {
  facilityName: 'City General Hospital',
  jobTitle: 'Emergency Department Nurse',
  countryId: 'country-sa',
  startDate: '2019-04-01',
  endDate: '2024-12-31',
  isCurrent: false,
  description: 'Triage and resuscitation.',
};

async function setup(inputs: {
  mode?: 'create' | 'edit';
  initial?: NurseExperienceFormValue;
}): Promise<ComponentFixture<NurseExperienceForm>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseExperienceForm],
    providers: [provideRouter([])],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseExperienceForm);
  fixture.componentRef.setInput('mode', inputs.mode ?? 'create');
  fixture.componentRef.setInput('countries', COUNTRIES);
  if (inputs.initial !== undefined) {
    fixture.componentRef.setInput('initial', inputs.initial);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function text(fixture: ComponentFixture<NurseExperienceForm>): string {
  return fixture.nativeElement.textContent as string;
}

describe('NurseExperienceForm', () => {
  it('renders the exact seven supported fields in create mode with empty defaults', async () => {
    const fixture = await setup({ mode: 'create' });
    const content = text(fixture);

    for (const label of [
      'Job title',
      'Facility name',
      'Country',
      'Start date',
      'I currently work in this role',
      'End date',
      'Description',
    ]) {
      expect(content).toContain(label);
    }
    expect(content).not.toContain('Employment type');
    expect(content).not.toContain('Salary');
    expect(content).not.toContain('Specialty');
  });

  it('prefills every field from the loaded record in edit mode', async () => {
    const fixture = await setup({ mode: 'edit', initial: EDIT_VALUE });
    const component = formOf(fixture);

    expect(component.form.controls.jobTitle.value).toBe('Emergency Department Nurse');
    expect(component.form.controls.facilityName.value).toBe('City General Hospital');
    expect(component.form.controls.countryId.value).toBe('country-sa');
    expect(component.form.controls.startDate.value).toBe('2019-04-01');
    expect(component.form.controls.endDate.value).toBe('2024-12-31');
    expect(component.form.controls.isCurrent.value).toBe(false);
    expect(component.form.controls.description.value).toBe('Triage and resuscitation.');
  });

  it('populates the country selector from the backend lookup without hardcoded options', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = fixture.componentInstance as unknown as {
      countryOptions: readonly { value: string; label: string }[];
    };

    expect(component.countryOptions).toEqual([
      { value: 'country-sa', label: 'Saudi Arabia' },
      { value: 'country-ca', label: 'Canada' },
    ]);
  });

  it('disables and clears End Date when Current is checked, restoring on uncheck', async () => {
    const fixture = await setup({ mode: 'edit', initial: EDIT_VALUE });
    const component = formOf(fixture);

    component.updateIsCurrent(true);
    fixture.detectChanges();

    expect(component.form.controls.endDate.disabled).toBe(true);
    expect(component.form.controls.endDate.value).toBe('');

    component.updateIsCurrent(false);
    fixture.detectChanges();

    expect(component.form.controls.endDate.enabled).toBe(true);
    expect(component.form.controls.endDate.value).toBe('2024-12-31');
  });

  it('emits the exact save payload with null-trimmed optionals on valid submit', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted: NurseExperienceFormValue | undefined;
    component.save.subscribe((value: NurseExperienceFormValue) => {
      emitted = value;
    });

    component.updateJobTitle('  ICU Nurse  ');
    component.updateFacilityName('City General Hospital');
    component.updateStartDate('2021-06-01');
    component.updateDescription('   ');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toEqual({
      facilityName: 'City General Hospital',
      jobTitle: 'ICU Nurse',
      countryId: null,
      startDate: '2021-06-01',
      endDate: null,
      isCurrent: false,
      description: null,
    });
  });

  it('clears End Date from the payload when Current is checked', async () => {
    const fixture = await setup({ mode: 'edit', initial: EDIT_VALUE });
    const component = formOf(fixture);
    let emitted: NurseExperienceFormValue | undefined;
    component.save.subscribe((value: NurseExperienceFormValue) => {
      emitted = value;
    });

    component.updateIsCurrent(true);
    await component.submit();

    expect(emitted?.endDate).toBeNull();
    expect(emitted?.isCurrent).toBe(true);
  });

  it('blocks submit with field errors when required fields are missing', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('Check the highlighted fields');
  });

  it('rejects an end date before the start date', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    component.updateJobTitle('ICU Nurse');
    component.updateFacilityName('City General Hospital');
    component.updateStartDate('2022-01-01');
    component.updateEndDate('2021-12-31');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('End date must be on or after the start date.');
  });

  it('keeps End Date optional when the role is not current', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted: NurseExperienceFormValue | undefined;
    component.save.subscribe((value: NurseExperienceFormValue) => {
      emitted = value;
    });

    component.updateJobTitle('ICU Nurse');
    component.updateFacilityName('City General Hospital');
    component.updateStartDate('2020-01-15');
    await component.submit();

    expect(emitted?.endDate).toBeNull();
    expect(text(fixture)).not.toContain('End date is required');
  });

  it('emits cancel without saving', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let cancelled = 0;
    component.cancelled.subscribe(() => {
      cancelled += 1;
    });

    component.cancel();

    expect(cancelled).toBe(1);
  });

  it('exposes DebugElement access to the shared date controls', async () => {
    const fixture = await setup({ mode: 'create' });
    const dates = fixture.debugElement.queryAll(By.css('np-date-control'));

    expect(dates.length).toBe(2);
  });
});
