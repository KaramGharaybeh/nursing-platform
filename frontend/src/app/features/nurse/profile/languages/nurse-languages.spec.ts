import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { LanguagesApi } from '../../../../core/api/languages-api';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { LanguageListItemDto } from '../../../../core/api/generated/models/language-list-item-dto';
import type { NurseLanguageDto } from '../../../../core/api/generated/models/nurse-language-dto';
import { NurseLanguages } from './nurse-languages';

const CATALOG: readonly LanguageListItemDto[] = [
  { id: 'lang-ar', name: 'Arabic', code: 'AR' },
  { id: 'lang-en', name: 'English', code: 'EN' },
  { id: 'lang-fr', name: 'French', code: 'FR' },
];

const SAVED: NurseLanguageDto[] = [
  { id: 'nl-1', languageId: 'lang-en', name: 'English', code: 'EN', proficiency: 'Fluent' },
  { id: 'nl-2', languageId: 'lang-ar', name: 'Arabic', code: 'AR', proficiency: 'Native' },
];

class LanguagesApiStub {
  catalog: readonly LanguageListItemDto[] = CATALOG;
  catalogError: unknown = undefined;

  list() {
    return this.catalogError === undefined ? of(this.catalog) : throwError(() => this.catalogError);
  }
}

class NurseLanguagesApiStub {
  saved: NurseLanguageDto[] = [...SAVED];
  listError: unknown = undefined;
  updateError: unknown = undefined;
  updated: { body: unknown }[] = [];

  listLanguages() {
    return this.listError === undefined ? of(this.saved) : throwError(() => this.listError);
  }

  updateLanguages(body: unknown) {
    this.updated.push({ body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    const languages = (body as { languages: { languageId: string; proficiency: string }[] }).languages;
    this.saved = languages.map((entry, index) => {
      const catalogEntry = CATALOG.find((item) => item.id === entry.languageId);
      return {
        id: `nl-${index}`,
        languageId: entry.languageId,
        name: catalogEntry?.name ?? '?',
        code: catalogEntry?.code ?? '?',
        proficiency: entry.proficiency,
      };
    });
    return of(this.saved);
  }
}

async function setup(options?: {
  languagesApi?: LanguagesApiStub;
  profileApi?: NurseLanguagesApiStub;
}): Promise<{ fixture: ComponentFixture<NurseLanguages>; api: NurseLanguagesApiStub; announcer: Announcer }> {
  const languagesApi = options?.languagesApi ?? new LanguagesApiStub();
  const profileApi = options?.profileApi ?? new NurseLanguagesApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseLanguages],
    providers: [
      provideRouter([]),
      { provide: LanguagesApi, useValue: languagesApi },
      { provide: NurseProfileApi, useValue: profileApi },
    ],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseLanguages);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: profileApi, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseLanguages>): string {
  return fixture.nativeElement.textContent as string;
}

function allByTestId(fixture: ComponentFixture<NurseLanguages>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseLanguages>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseLanguages', () => {
  it('loads saved languages with authoritative catalog names and exact proficiencies', async () => {
    const { fixture } = await setup();
    const rows = allByTestId(fixture, 'language-row');
    const component = fixture.componentInstance as unknown as {
      rows(): { languageId: string; proficiency: string }[];
      proficiencyOptions: readonly { value: string; label: string }[];
    };

    expect(rows.length).toBe(2);
    expect(component.rows()).toEqual([
      { languageId: 'lang-en', proficiency: 'Fluent' },
      { languageId: 'lang-ar', proficiency: 'Native' },
    ]);
    expect(component.proficiencyOptions.map((option) => option.value)).toEqual([
      'Beginner',
      'Intermediate',
      'Advanced',
      'Fluent',
      'Native',
    ]);
    expect(text(fixture)).toContain('English');
    expect(text(fixture)).not.toContain('Bilingual');
    expect(text(fixture)).not.toContain('Conversational');
  });

  it('shows an empty state when no languages are saved', async () => {
    const profileApi = new NurseLanguagesApiStub();
    profileApi.saved = [];
    const { fixture } = await setup({ profileApi });

    expect(text(fixture)).toContain('No languages added yet.');
  });

  it('adds an incomplete row with no preselected language or proficiency', async () => {
    const { fixture } = await setup();
    const component = fixture.componentInstance as unknown as {
      addRow(): void;
      rows(): { languageId: string; proficiency: string }[];
    };

    component.addRow();
    fixture.detectChanges();

    expect(component.rows().length).toBe(3);
    expect(component.rows()[2]).toEqual({ languageId: '', proficiency: '' });
  });

  it('blocks save while a row is incomplete', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      addRow(): void;
      updateRowLanguage(index: number, value: string): void;
      save(): Promise<void>;
    };

    component.addRow();
    component.updateRowLanguage(2, 'lang-fr');
    await component.save();
    fixture.detectChanges();

    expect(api.updated.length).toBe(0);
    expect(text(fixture)).toContain('Select a proficiency');
  });

  it('rejects duplicate language selections', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      addRow(): void;
      updateRowLanguage(index: number, value: string): void;
      updateRowProficiency(index: number, value: string): void;
      save(): Promise<void>;
    };

    component.addRow();
    component.updateRowLanguage(2, 'lang-en');
    component.updateRowProficiency(2, 'Beginner');
    await component.save();
    fixture.detectChanges();

    expect(api.updated.length).toBe(0);
    expect(text(fixture)).toContain('has already been added');
  });

  it('saves the complete collection and replaces local truth from the response', async () => {
    const { fixture, api, announcer } = await setup();
    const component = fixture.componentInstance as unknown as {
      addRow(): void;
      updateRowLanguage(index: number, value: string): void;
      updateRowProficiency(index: number, value: string): void;
      save(): Promise<void>;
    };

    component.addRow();
    component.updateRowLanguage(2, 'lang-fr');
    component.updateRowProficiency(2, 'Intermediate');
    await component.save();
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({
      languages: [
        { languageId: 'lang-en', proficiency: 'Fluent' },
        { languageId: 'lang-ar', proficiency: 'Native' },
        { languageId: 'lang-fr', proficiency: 'Intermediate' },
      ],
    });
    expect(text(fixture)).toContain('French');
    expect(announcer.current()?.text).toBe('Languages saved.');
  });

  it('removes a row locally and saves without it', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      removeRow(index: number): void;
      save(): Promise<void>;
    };

    component.removeRow(0);
    await component.save();
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({ languages: [{ languageId: 'lang-ar', proficiency: 'Native' }] });
  });

  it('saves an empty collection to clear all languages', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      removeRow(index: number): void;
      save(): Promise<void>;
    };

    component.removeRow(0);
    component.removeRow(0);
    await component.save();
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].body).toEqual({ languages: [] });
    expect(text(fixture)).toContain('No languages added yet.');
  });

  it('cancels edits back to the last-saved state without calling the backend', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      addRow(): void;
      cancel(): void;
      rows(): unknown[];
    };

    component.addRow();
    component.cancel();
    fixture.detectChanges();

    expect(component.rows().length).toBe(2);
    expect(api.updated.length).toBe(0);
  });

  it('surfaces a clear message when save fails with invalid-language 409', async () => {
    const profileApi = new NurseLanguagesApiStub();
    profileApi.updateError = {
      status: 409,
      error: { title: 'Conflict', status: 409, detail: 'One or more languages are invalid or inactive.' },
    };
    const { fixture } = await setup({ profileApi });
    const component = fixture.componentInstance as unknown as { save(): Promise<void> };

    await component.save();
    await settle(fixture);

    expect(text(fixture)).toContain('no longer available');
  });

  it('retries the catalog load after lookup failure', async () => {
    const languagesApi = new LanguagesApiStub();
    languagesApi.catalogError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup({ languagesApi });

    expect(text(fixture)).toContain('Try again');
    expect(allByTestId(fixture, 'language-row').length).toBe(0);
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse languages route', () => {
  it('mounts /nurse/profile/languages with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/languages');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_LANGUAGES' });
  });
});
