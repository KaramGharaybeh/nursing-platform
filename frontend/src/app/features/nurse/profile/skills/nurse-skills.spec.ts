import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseSkillDto } from '../../../../core/api/generated/models/nurse-skill-dto';
import { NurseSkills } from './nurse-skills';

const SAVED: NurseSkillDto[] = [
  { id: 'skill-1', name: 'Triage' },
  { id: 'skill-2', name: 'Critical Care' },
];

class SkillsApiStub {
  saved: NurseSkillDto[] = [...SAVED];
  listError: unknown = undefined;
  updateError: unknown = undefined;
  updated: { body: unknown }[] = [];

  listSkills() {
    return this.listError === undefined ? of(this.saved) : throwError(() => this.listError);
  }

  updateSkills(body: unknown) {
    this.updated.push({ body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    const names = (body as { skills: string[] }).skills;
    this.saved = names.map((name, index) => ({ id: `skill-${index}`, name }));
    return of(this.saved);
  }
}

async function setup(api?: SkillsApiStub): Promise<{ fixture: ComponentFixture<NurseSkills>; api: SkillsApiStub; announcer: Announcer }> {
  const stub = api ?? new SkillsApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseSkills],
    providers: [provideRouter([]), { provide: NurseProfileApi, useValue: stub }],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseSkills);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseSkills>): string {
  return fixture.nativeElement.textContent as string;
}

function allByTestId(fixture: ComponentFixture<NurseSkills>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseSkills>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseSkills', () => {
  it('loads and renders saved skills as removable chips without taxonomy UI', async () => {
    const { fixture } = await setup();
    const chips = allByTestId(fixture, 'skill-chip');

    expect(chips.length).toBe(2);
    expect(text(fixture)).toContain('Triage');
    expect(text(fixture)).toContain('Critical Care');
    expect(text(fixture)).not.toContain('category');
    expect(text(fixture).toLowerCase()).not.toContain('autocomplete');
  });

  it('shows an empty state when no skills are saved', async () => {
    const api = new SkillsApiStub();
    api.saved = [];
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No skills added yet.');
  });

  it('adds a skill through the Add action with whitespace normalization', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      draftSkills(): string[];
    };

    component.updateInput('  ICU   Nursing  ');
    component.addSkill();
    fixture.detectChanges();

    expect(component.draftSkills()).toContain('ICU Nursing');
    expect(text(fixture)).toContain('ICU Nursing');
  });

  it('rejects blank skills', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      draftSkills(): string[];
    };

    component.updateInput('   ');
    component.addSkill();
    fixture.detectChanges();

    expect(component.draftSkills().length).toBe(2);
    expect(text(fixture)).toContain('Enter a skill name.');
  });

  it('rejects exact and case-insensitive duplicates', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      draftSkills(): string[];
    };

    for (const duplicate of ['Triage', 'triage', 'TRIAGE  ']) {
      component.updateInput(duplicate);
      component.addSkill();
      fixture.detectChanges();
      expect(component.draftSkills().length).toBe(2);
    }
    expect(text(fixture)).toContain('has already been added');
  });

  it('rejects skills longer than 100 characters', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      draftSkills(): string[];
    };

    component.updateInput('a'.repeat(101));
    component.addSkill();
    fixture.detectChanges();

    expect(component.draftSkills().length).toBe(2);
    expect(text(fixture)).toContain('at most 100 characters');
  });

  it('accepts a 100-character skill', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      draftSkills(): string[];
    };

    component.updateInput('a'.repeat(100));
    component.addSkill();
    fixture.detectChanges();

    expect(component.draftSkills().length).toBe(3);
  });

  it('removes a skill locally without calling the backend', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      removeSkill(name: string): void;
      draftSkills(): string[];
    };

    component.removeSkill('Triage');
    fixture.detectChanges();

    expect(component.draftSkills()).toEqual(['Critical Care']);
    expect(api.updated.length).toBe(0);
  });

  it('saves the full collection and replaces local truth from the response', async () => {
    const { fixture, api, announcer } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      save(): Promise<void>;
      draftSkills(): string[];
    };

    component.updateInput('Wound Care');
    component.addSkill();
    await component.save();
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({ skills: ['Triage', 'Critical Care', 'Wound Care'] });
    expect(announcer.current()?.text).toBe('Skills saved.');
  });

  it('saves an empty collection to clear all skills', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      removeSkill(name: string): void;
      save(): Promise<void>;
    };

    component.removeSkill('Triage');
    component.removeSkill('Critical Care');
    await component.save();
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({ skills: [] });
    expect(text(fixture)).toContain('No skills added yet.');
  });

  it('resets the draft to the last-saved state on cancel', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      cancel(): void;
      draftSkills(): string[];
    };

    component.updateInput('Wound Care');
    component.addSkill();
    component.cancel();
    fixture.detectChanges();

    expect(component.draftSkills()).toEqual(['Triage', 'Critical Care']);
    expect(api.updated.length).toBe(0);
  });

  it('surfaces backend validation failures without losing the draft', async () => {
    const api = new SkillsApiStub();
    api.updateError = {
      status: 400,
      error: { title: 'Validation failed', status: 400, errors: { Skills: ['Duplicate skill names are not allowed.'] } },
    };
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      updateInput(value: string): void;
      addSkill(): void;
      save(): Promise<void>;
      draftSkills(): string[];
    };

    component.updateInput('Wound Care');
    component.addSkill();
    await component.save();
    await settle(fixture);

    expect(component.draftSkills().length).toBe(3);
    expect(text(fixture)).toContain('Duplicate skill names are not allowed.');
  });

  it('renders an error and retry affordance on load failure', async () => {
    const api = new SkillsApiStub();
    api.listError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
    expect(allByTestId(fixture, 'skill-chip').length).toBe(0);
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse skills route', () => {
  it('mounts /nurse/profile/skills with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/skills');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_SKILLS' });
  });
});
