import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { LocaleDirectionService } from '../../../core/locale/locale-direction.service';
import { NpLanguageSwitcher } from './language-switcher';

describe('NpLanguageSwitcher', () => {
  afterEach(() => {
    localStorage.removeItem('np-locale');
    TestBed.resetTestingModule();
    document.documentElement.lang = 'en';
    document.documentElement.dir = 'ltr';
  });

  async function setup() {
    await TestBed.configureTestingModule({
      imports: [NpLanguageSwitcher],
      providers: [provideRouter([])],
    }).compileComponents();
    const fixture = TestBed.createComponent(NpLanguageSwitcher);
    fixture.detectChanges();
    return { fixture, locale: TestBed.inject(LocaleDirectionService) };
  }

  function buttons(fixture: { nativeElement: HTMLElement }): HTMLButtonElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('button'));
  }

  it('exposes English and Arabic options with a clear accessible group name', async () => {
    const { fixture } = await setup();
    const root = fixture.nativeElement as HTMLElement;
    const options = buttons(fixture);

    expect(root.querySelector('[role="group"]')?.getAttribute('aria-label')).toBe('Language');
    expect(options).toHaveLength(2);
    expect(options.map((button) => button.textContent?.trim())).toEqual(['English', 'العربية']);
    expect(options.every((button) => button.type === 'button')).toBe(true);
  });

  it('switches locale, document direction, and visible copy without navigating', async () => {
    const { fixture, locale } = await setup();
    const arabic = buttons(fixture)[1] as HTMLButtonElement;

    expect(arabic?.getAttribute('aria-pressed')).toBe('false');

    arabic?.click();
    fixture.detectChanges();

    expect(locale.locale()).toBe('ar');
    expect(document.documentElement.lang).toBe('ar');
    expect(document.documentElement.dir).toBe('rtl');
    expect(localStorage.getItem('np-locale')).toBe('ar');
    expect(arabic?.getAttribute('aria-pressed')).toBe('true');
    expect(fixture.nativeElement.querySelector('[role="group"]')?.getAttribute('aria-label')).toBe(
      'اللغة',
    );
  });

  it('restores English state when English is selected', async () => {
    const { fixture, locale } = await setup();
    const [english, arabic] = buttons(fixture);

    arabic?.click();
    english?.click();
    fixture.detectChanges();

    expect(locale.locale()).toBe('en');
    expect(document.documentElement.lang).toBe('en');
    expect(document.documentElement.dir).toBe('ltr');
  });
});
