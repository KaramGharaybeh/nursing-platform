import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import {
  NurseCertificateForm,
  type NurseCertificateFormValue,
} from './nurse-certificate-form';

interface CertificateFormLike {
  form: {
    controls: Record<
      'name' | 'issuingOrganization' | 'issueDate' | 'expirationDate' | 'credentialId' | 'credentialUrl',
      { value: string; disabled: boolean; enabled: boolean }
    >;
  };
  updateName(value: string): void;
  updateIssuingOrganization(value: string): void;
  updateIssueDate(value: string): void;
  updateExpirationDate(value: string): void;
  updateCredentialId(value: string): void;
  updateCredentialUrl(value: string): void;
  submit(): Promise<void>;
  cancel(): void;
  save: { subscribe(next: (value: NurseCertificateFormValue) => void): void };
  cancelled: { subscribe(next: () => void): void };
}

function formOf(fixture: ComponentFixture<NurseCertificateForm>): CertificateFormLike {
  return fixture.componentInstance as unknown as CertificateFormLike;
}

const EDIT_VALUE: NurseCertificateFormValue = {
  name: 'Basic Life Support',
  issuingOrganization: 'American Heart Association',
  issueDate: '2023-03-15',
  expirationDate: '2025-03-15',
  credentialId: 'BLS-998877',
  credentialUrl: 'https://example.org/credentials/bls-998877',
};

async function setup(inputs: {
  mode?: 'create' | 'edit';
  initial?: NurseCertificateFormValue;
}): Promise<ComponentFixture<NurseCertificateForm>> {
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseCertificateForm],
    providers: [provideRouter([])],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseCertificateForm);
  fixture.componentRef.setInput('mode', inputs.mode ?? 'create');
  if (inputs.initial !== undefined) {
    fixture.componentRef.setInput('initial', inputs.initial);
  }
  fixture.detectChanges();
  await fixture.whenStable();
  return fixture;
}

function text(fixture: ComponentFixture<NurseCertificateForm>): string {
  return fixture.nativeElement.textContent as string;
}

describe('NurseCertificateForm', () => {
  it('renders the exact six supported fields in create mode with empty defaults', async () => {
    const fixture = await setup({ mode: 'create' });
    const content = text(fixture);

    for (const label of [
      'Certificate name',
      'Issuing organization',
      'Credential ID',
      'Issue date',
      'Expiration date',
      'Credential URL',
    ]) {
      expect(content).toContain(label);
    }
    expect(content).not.toContain('Description');
    expect(content).not.toContain('Verification status');
    expect(content).not.toContain('GPA');
  });

  it('prefills every field from the loaded record in edit mode', async () => {
    const fixture = await setup({ mode: 'edit', initial: EDIT_VALUE });
    const component = formOf(fixture);

    expect(component.form.controls.name.value).toBe('Basic Life Support');
    expect(component.form.controls.issuingOrganization.value).toBe('American Heart Association');
    expect(component.form.controls.issueDate.value).toBe('2023-03-15');
    expect(component.form.controls.expirationDate.value).toBe('2025-03-15');
    expect(component.form.controls.credentialId.value).toBe('BLS-998877');
    expect(component.form.controls.credentialUrl.value).toBe('https://example.org/credentials/bls-998877');
  });

  it('emits the exact save payload with null-trimmed optionals on valid submit', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted: NurseCertificateFormValue | undefined;
    component.save.subscribe((value: NurseCertificateFormValue) => {
      emitted = value;
    });

    component.updateName('  Basic Life Support  ');
    component.updateIssuingOrganization('American Heart Association');
    component.updateCredentialId('   ');
    component.updateCredentialUrl('  ');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toEqual({
      name: 'Basic Life Support',
      issuingOrganization: 'American Heart Association',
      issueDate: null,
      expirationDate: null,
      credentialId: null,
      credentialUrl: null,
    });
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

  it('rejects an expiration date before the issue date only when both are provided', async () => {
    const fixture = await setup({ mode: 'create' });
    const component = formOf(fixture);
    let emitted = 0;
    component.save.subscribe(() => {
      emitted += 1;
    });

    component.updateName('Basic Life Support');
    component.updateIssuingOrganization('American Heart Association');
    component.updateIssueDate('2024-01-01');
    component.updateExpirationDate('2023-12-31');
    await component.submit();
    fixture.detectChanges();

    expect(emitted).toBe(0);
    expect(text(fixture)).toContain('Expiration date must be on or after the issue date.');
  });

  it('accepts http and https credential URLs', async () => {
    for (const url of ['http://example.org/cred/1', 'https://example.org/cred/1']) {
      const fixture = await setup({ mode: 'create' });
      const component = formOf(fixture);
      let emitted: NurseCertificateFormValue | undefined;
      component.save.subscribe((value: NurseCertificateFormValue) => {
        emitted = value;
      });

      component.updateName('Basic Life Support');
      component.updateIssuingOrganization('American Heart Association');
      component.updateCredentialUrl(url);
      await component.submit();

      expect(emitted?.credentialUrl).toBe(url);
    }
  });

  it('rejects relative URLs and unsupported schemes', async () => {
    for (const url of ['/credentials/1', 'ftp://example.org/cred/1', 'javascript:alert(1)']) {
      const fixture = await setup({ mode: 'create' });
      const component = formOf(fixture);
      let emitted = 0;
      component.save.subscribe(() => {
        emitted += 1;
      });

      component.updateName('Basic Life Support');
      component.updateIssuingOrganization('American Heart Association');
      component.updateCredentialUrl(url);
      await component.submit();
      fixture.detectChanges();

      expect(emitted).toBe(0);
      expect(text(fixture)).toContain('Credential URL must be an absolute http or https URL.');
    }
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
