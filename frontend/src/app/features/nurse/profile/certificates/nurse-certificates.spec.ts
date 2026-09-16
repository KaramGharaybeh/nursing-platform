import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseCertificateDto } from '../../../../core/api/generated/models/nurse-certificate-dto';
import { NurseCertificates } from './nurse-certificates';
import type { NurseCertificateFormValue } from './nurse-certificate-form';

const DATED: NurseCertificateDto = {
  id: 'cert-1',
  name: 'Basic Life Support',
  issuingOrganization: 'American Heart Association',
  issueDate: '2023-03-15',
  expirationDate: '2025-03-15',
  credentialId: 'BLS-998877',
  credentialUrl: 'https://example.org/credentials/bls-998877',
};

const OPEN: NurseCertificateDto = {
  id: 'cert-2',
  name: 'Advanced Cardiac Life Support',
  issuingOrganization: 'American Heart Association',
  issueDate: '2024-06-01',
  expirationDate: null,
  credentialId: null,
  credentialUrl: null,
};

const DATELESS: NurseCertificateDto = {
  id: 'cert-3',
  name: 'Infection Control Certificate',
  issuingOrganization: 'Harbor Institute',
  issueDate: null,
  expirationDate: null,
  credentialId: null,
  credentialUrl: 'https://example.org/credentials/icc-112233',
};

class CertificatesApiStub {
  records: NurseCertificateDto[] = [DATED, OPEN, DATELESS];
  listError: unknown = undefined;
  updateError: unknown = undefined;
  deleteError: unknown = undefined;
  created: { body: unknown }[] = [];
  updated: { id: string; body: unknown }[] = [];
  deleted: string[] = [];

  listCertificates() {
    return this.listError === undefined ? of(this.records) : throwError(() => this.listError);
  }

  createCertificate(body: unknown) {
    this.created.push({ body });
    const created: NurseCertificateDto = {
      id: 'cert-new',
      name: 'Pediatric Advanced Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: '2024-01-10',
      expirationDate: '2026-01-10',
      credentialId: 'PALS-445566',
      credentialUrl: null,
    };
    this.records = [created, ...this.records];
    return of(created);
  }

  updateCertificate(id: string, body: unknown) {
    this.updated.push({ id, body });
    if (this.updateError !== undefined) {
      return throwError(() => this.updateError);
    }
    return of({ ...DATED, id });
  }

  deleteCertificate(id: string) {
    this.deleted.push(id);
    if (this.deleteError !== undefined) {
      return throwError(() => this.deleteError);
    }
    this.records = this.records.filter((record) => record.id !== id);
    return of(undefined);
  }
}

async function setup(api?: CertificatesApiStub): Promise<{ fixture: ComponentFixture<NurseCertificates>; api: CertificatesApiStub; announcer: Announcer }> {
  const stub = api ?? new CertificatesApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseCertificates],
    providers: [provideRouter([]), { provide: NurseProfileApi, useValue: stub }],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseCertificates);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseCertificates>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<NurseCertificates>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

function allByTestId(fixture: ComponentFixture<NurseCertificates>, id: string): HTMLElement[] {
  return Array.from(fixture.nativeElement.querySelectorAll(`[data-testid="${id}"]`)) as HTMLElement[];
}

async function settle(fixture: ComponentFixture<NurseCertificates>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseCertificates', () => {
  it('renders records in backend order with supported fields only', async () => {
    const { fixture } = await setup();
    const content = text(fixture);
    const cards = allByTestId(fixture, 'certificate-card');

    expect(cards.length).toBe(3);
    expect(cards[0].textContent).toContain('Basic Life Support');
    expect(cards[1].textContent).toContain('Advanced Cardiac Life Support');
    expect(cards[2].textContent).toContain('Infection Control Certificate');
    expect(content).toContain('American Heart Association');
    expect(content).not.toContain('Verified');
    expect(content).not.toContain('GPA');
    expect(content).not.toContain('Accreditation');
  });

  it('renders credential IDs, safe external links, and date lines without status', async () => {
    const { fixture } = await setup();
    const cards = allByTestId(fixture, 'certificate-card');

    expect(cards[0].textContent).toContain('BLS-998877');
    expect(cards[1].textContent).not.toContain('Credential ID');
    const link = cards[0].querySelector('a[data-testid="certificate-link"]') as HTMLAnchorElement | null;
    expect(link?.textContent?.trim()).toBe('Open credential link');
    expect(link?.getAttribute('href')).toBe('https://example.org/credentials/bls-998877');
    expect(link?.getAttribute('target')).toBe('_blank');
    expect(link?.getAttribute('rel')).toContain('noopener');
    expect(link?.getAttribute('rel')).toContain('noreferrer');
    expect(cards[1].querySelector('a[data-testid="certificate-link"]')).toBeNull();
    expect(cards[0].textContent).toContain('Mar 15, 2023');
    expect(cards[0].textContent).toContain('Mar 15, 2025');
    expect(cards[1].textContent).toContain('Jun 1, 2024');
    expect(cards[2].querySelector('[data-testid="certificate-dates"]')).toBeNull();
    expect(text(fixture)).not.toContain('Expires');
    expect(text(fixture)).not.toContain('Expired');
    expect(text(fixture)).not.toContain('Valid');
    expect(text(fixture)).not.toContain('Active');
  });

  it('shows a calm empty state with Add certificate switching to create mode on the same route', async () => {
    const api = new CertificatesApiStub();
    api.records = [];
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No certificates added yet.');
    byTestId(fixture, 'add-certificate')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(text(fixture)).toContain('Add certificate');
    expect(fixture.nativeElement.querySelector('np-nurse-certificate-form')).not.toBeNull();
  });

  it('creates a record with the exact request body then returns to the refreshed list', async () => {
    const { fixture, api, announcer } = await setup();
    byTestId(fixture, 'add-certificate')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-certificate-form',
    ).componentInstance as {
      save: { emit(value: NurseCertificateFormValue): void };
    };
    form.save.emit({
      name: 'Pediatric Advanced Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: '2024-01-10',
      expirationDate: '2026-01-10',
      credentialId: 'PALS-445566',
      credentialUrl: null,
    });
    await settle(fixture);

    expect(api.created.length).toBe(1);
    expect(api.created[0].body).toEqual({
      name: 'Pediatric Advanced Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: '2024-01-10',
      expirationDate: '2026-01-10',
      credentialId: 'PALS-445566',
      credentialUrl: null,
    });
    expect(text(fixture)).toContain('Pediatric Advanced Life Support');
    expect(announcer.current()?.text).toBe('Certificate saved.');
  });

  it('cancels create mode without calling the backend', async () => {
    const { fixture, api } = await setup();
    byTestId(fixture, 'add-certificate')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-certificate-form',
    ).componentInstance as { cancelled: { emit(): void } };
    form.cancelled.emit();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.created.length).toBe(0);
    expect(allByTestId(fixture, 'certificate-card').length).toBe(3);
  });

  it('prefills the edit form from the loaded record and updates by id', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'certificate-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-certificate-form',
    ).componentInstance as {
      initial: NurseCertificateFormValue | undefined;
      save: { emit(value: NurseCertificateFormValue): void };
    };
    expect(form.initial).toEqual({
      name: 'Basic Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: '2023-03-15',
      expirationDate: '2025-03-15',
      credentialId: 'BLS-998877',
      credentialUrl: 'https://example.org/credentials/bls-998877',
    });

    form.save.emit({ ...(form.initial as NurseCertificateFormValue), name: 'BLS Provider' });
    await settle(fixture);

    expect(api.updated.length).toBe(1);
    expect(api.updated[0].id).toBe('cert-1');
    expect((api.updated[0].body as { name: string }).name).toBe('BLS Provider');
    expect(fixture.nativeElement.querySelector('np-nurse-certificate-form')).toBeNull();
  });

  it('shows a record-missing message when update returns 404', async () => {
    const api = new CertificatesApiStub();
    api.updateError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'certificate-edit')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    const form = fixture.debugElement.query(
      (node) => node.name === 'np-nurse-certificate-form',
    ).componentInstance as {
      save: { emit(value: NurseCertificateFormValue): void };
    };
    form.save.emit({
      name: 'Basic Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: null,
      expirationDate: null,
      credentialId: null,
      credentialUrl: null,
    });
    await settle(fixture);

    expect(text(fixture)).toContain('no longer exists');
  });

  it('requires a second explicit action before deleting and never deletes on first click', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'certificate-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(text(fixture)).toContain('This cannot be undone.');
    expect(byTestId(fixture, 'certificate-confirm-delete')).not.toBeNull();
  });

  it('cancels the inline confirmation with Keep without mutating', async () => {
    const { fixture, api } = await setup();
    allByTestId(fixture, 'certificate-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'certificate-keep')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted.length).toBe(0);
    expect(byTestId(fixture, 'certificate-confirm-delete')).toBeNull();
    expect(allByTestId(fixture, 'certificate-card').length).toBe(3);
  });

  it('deletes on confirm, removes the card, and announces the outcome', async () => {
    const { fixture, api, announcer } = await setup();
    allByTestId(fixture, 'certificate-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'certificate-confirm-delete')?.click();
    await settle(fixture);

    expect(api.deleted).toEqual(['cert-1']);
    expect(allByTestId(fixture, 'certificate-card').length).toBe(2);
    expect(announcer.current()?.text).toBe('Certificate deleted.');
  });

  it('treats delete 404 as already removed with a calm notice', async () => {
    const api = new CertificatesApiStub();
    api.deleteError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture, announcer } = await setup(api);
    allByTestId(fixture, 'certificate-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'certificate-confirm-delete')?.click();
    await settle(fixture);

    expect(text(fixture)).toContain('already been removed');
    expect(announcer.current()?.text).toBe('Certificate was already removed.');
  });

  it('shows an inline retryable error when delete fails generically', async () => {
    const api = new CertificatesApiStub();
    api.deleteError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    allByTestId(fixture, 'certificate-delete')[0].click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'certificate-confirm-delete')?.click();
    await settle(fixture);

    expect(allByTestId(fixture, 'certificate-card').length).toBe(3);
    expect(text(fixture)).toContain('could not be deleted');
  });

  it('renders an error and retry affordance on load failure', async () => {
    const api = new CertificatesApiStub();
    api.listError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
    expect(allByTestId(fixture, 'certificate-card').length).toBe(0);
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse certificates route', () => {
  it('mounts /nurse/profile/certificates with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/certificates');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_CERTIFICATES' });
  });
});
