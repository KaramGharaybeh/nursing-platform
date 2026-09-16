import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import {
  PersonalDetailsForm,
  type PersonalDetailsFormValue,
} from './personal-details-form';

interface PersonalDetailsFormLike {
  updateFirstName(value: string): void;
  updateLastName(value: string): void;
  submit(): Promise<void>;
  cancel(): void;
  save: { subscribe(next: (value: PersonalDetailsFormValue) => void): void };
  cancelled: { subscribe(next: () => void): void };
}

function formOf(fixture: ComponentFixture<PersonalDetailsForm>): PersonalDetailsFormLike {
  return fixture.componentInstance as unknown as PersonalDetailsFormLike;
}

async function setup(initial?: PersonalDetailsFormValue): Promise<ComponentFixture<PersonalDetailsForm>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [PersonalDetailsForm],
    providers: [provideRouter([])],
  }).compileComponents();
  const fixture = TestBed.createComponent(PersonalDetailsForm);
  if (initial !== undefined) {
    fixture.componentRef.setInput('initial', initial);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function text(fixture: ComponentFixture<PersonalDetailsForm>): string {
  return fixture.nativeElement.textContent as string;
}

describe('PersonalDetailsForm', () => {
  it('renders only first and last name fields', async () => {
    const fixture = await setup({ firstName: 'Nadia', lastName: 'Nursefield' });
    const content = text(fixture);

    expect(content).toContain('First name');
    expect(content).toContain('Last name');
    expect(content).not.toContain('Username');
    expect(content).not.toContain('Email');
    expect(content).not.toContain('Password');
    expect(content).not.toContain('Role');
  });

  it('prefills provided names', async () => {
    const fixture = await setup({ firstName: 'Nadia', lastName: 'Nursefield' });
    const inputs = Array.from(
      fixture.nativeElement.querySelectorAll('input'),
    ) as HTMLInputElement[];

    expect(inputs.map((input) => input.value)).toEqual(['Nadia', 'Nursefield']);
  });

  it('emits trimmed names on valid submit', async () => {
    const fixture = await setup();
    const component = formOf(fixture);
    let emitted: PersonalDetailsFormValue | undefined;
    component.save.subscribe((value: PersonalDetailsFormValue) => {
      emitted = value;
    });

    component.updateFirstName('  Nora  ');
    component.updateLastName('Nursing');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toEqual({ firstName: 'Nora', lastName: 'Nursing' });
  });

  it('blocks blank names without emitting', async () => {
    const fixture = await setup();
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    component.updateFirstName('   ');
    component.updateLastName('Nursing');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('Check the highlighted fields');
  });

  it('rejects names longer than 100 characters', async () => {
    const fixture = await setup();
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    component.updateFirstName('a'.repeat(101));
    component.updateLastName('Nursing');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('at most 100 characters');
  });

  it('emits cancel without saving', async () => {
    const fixture = await setup();
    const component = formOf(fixture);
    let cancelled = 0;
    component.cancelled.subscribe(() => {
      cancelled += 1;
    });

    component.cancel();

    expect(cancelled).toBe(1);
  });
});
