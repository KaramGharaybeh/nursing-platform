import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import type { CountryListItemDto } from '../../../../core/api/generated/models/country-list-item-dto';
import {
  NurseEducationForm,
  type NurseEducationFormValue,
} from './nurse-education-form';

interface EducationFormLike {
  form: {
    controls: Record<
      'institutionName' | 'degree' | 'fieldOfStudy' | 'countryId' | 'startDate' | 'endDate' | 'description',
      { value: string; disabled: boolean; enabled: boolean }
    >;
  };
  updateInstitutionName(value: string): void;
  updateDegree(value: string): void;
  updateFieldOfStudy(value: string): void;
  updateCountry(value: string): void;
  updateStartDate(value: string): void;
  updateEndDate(value: string): void;
  updateDescription(value: string): void;
  submit(): Promise<void>;
  cancel(): void;
  save: { subscribe(next: (value: NurseEducationFormValue) => void): void };
  cancelled: { subscribe(next: () => void): void };
}

function formOf(fixture: ComponentFixture<NurseEducationForm>): EducationFormLike {
  return fixture.componentInstance as unknown as EducationFormLike;
}

const COUNTRIES: readonly CountryListItemDto[] = [
  { id: 'country-sa', name: 'Saudi Arabia', code: 'SA' },
  { id: 'country-ca', name: 'Canada', code: 'CA' },
];

const EDIT_VALUE: NurseEducationFormValue = {
  institutionName: 'Riyadh College of Nursing',
  degree: 'Bachelor of Science in Nursing',
  fieldOfStudy: 'Critical Care Nursing',
  countryId: 'country-sa',
  startDate: '2015-09-01',
  endDate: '2019-06-30',
  description: 'Clinical rotations across emergency and intensive care.',
};

async function setup(inputs: {
  mode?: 'create' | 'edit';
  initial?: NurseEducationFormValue;
}): Promise<ComponentFixture<NurseEducationForm>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseEducationForm],
    providers: [provideRouter([])],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseEducationForm);
  fixture.componentRef.setInput('mode', inputs.mode ?? 'create');
  fixture.componentRef.setInput('countries', COUNTRIES);
  if (inputs.initial !== undefined) {
    fixture.componentRef.setInput('initial', inputs.initial);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function text(fixture: ComponentFixture<NurseEducationForm>): string {
  return fixture.nativeElement.textContent as string;
}

describe('NurseEducationForm', () => {
  it('renders the exact seven supported fields in create mode with empty defaults', async () => {
    const fixture = await setup({ mode: 'create' });
    const content = text(fixture);

    for (const label of [
      'Degree',
      'Field of study',
      'Institution name',
      'Country',
      'Start date',
      'End date',
      'Description',
    ]) {
      expect(content).toContain(label);
    }
    expect(content).not.toContain('GPA');
    expect(content).not.toContain('Honors');
    expect(content).not.toContain('Accreditation');
    expect(content).not.toContain('Transcript');
  });

  it('prefills every field from the loaded record in edit mode', async () => {
    const fixture = await setup({ mode: 'edit', initial: EDIT_VALUE });
    const component = formOf(fixture);

    expect(component.form.controls.degree.value).toBe('Bachelor of Science in Nursing');
    expect(component.form.controls.fieldOfStudy.value).toBe('Critical Care Nursing');
    expect(component.form.controls.institutionName.value).toBe('Riyadh College of Nursing');
    expect(component.form.controls.countryId.value).toBe('country-sa');
    expect(component.form.controls.startDate.value).toBe('2015-09-01');
    expect(component.form.controls.endDate.value).toBe('2019-06-30');
    expect(component.form.controls.description.value).toBe(
      'Clinical rotations across emergency and intensive care.',
    );
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

  it('emits the exact save payload with null-trimmed optionals on valid submit', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted: NurseEducationFormValue | undefined;
    component.save.subscribe((value: NurseEducationFormValue) => {
      emitted = value;
    });

    component.updateDegree('  Bachelor of Science in Nursing  ');
    component.updateInstitutionName('Riyadh College of Nursing');
    component.updateFieldOfStudy('   ');
    component.updateDescription('');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toEqual({
      institutionName: 'Riyadh College of Nursing',
      degree: 'Bachelor of Science in Nursing',
      fieldOfStudy: null,
      countryId: null,
      startDate: null,
      endDate: null,
      description: null,
    });
  });

  it('allows fully dateless education records', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted: NurseEducationFormValue | undefined;
    component.save.subscribe((value: NurseEducationFormValue) => {
      emitted = value;
    });

    component.updateDegree('Diploma in Nursing');
    component.updateInstitutionName('Harbor Institute');
    await component.submit();

    expect(emitted?.startDate).toBeNull();
    expect(emitted?.endDate).toBeNull();
    expect(text(fixture)).not.toContain('Start date is required');
    expect(text(fixture)).not.toContain('End date is required');
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

  it('rejects an end date before the start date only when both are provided', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    component.updateDegree('Bachelor of Science in Nursing');
    component.updateInstitutionName('Riyadh College of Nursing');
    component.updateStartDate('2022-01-01');
    component.updateEndDate('2021-12-31');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('End date must be on or after the start date.');
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
