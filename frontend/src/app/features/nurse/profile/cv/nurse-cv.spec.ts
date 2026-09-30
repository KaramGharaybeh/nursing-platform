import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../../../../app.routes';
import { Announcer } from '../../../../shared/ui/announcement';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseCvDocumentDto } from '../../../../core/api/generated/models/nurse-cv-document-dto';
import { NurseCv } from './nurse-cv';

const CV: NurseCvDocumentDto = {
  id: 'cv-1',
  fileName: 'nurse-cv.pdf',
  contentType: 'application/pdf',
  fileSizeBytes: 2048,
  uploadedAt: '2026-09-16T10:00:00Z',
};

function pdfFile(name = 'nurse-cv.pdf', size = 2048): File {
  return new File([new Uint8Array(size)], name, { type: 'application/pdf' });
}

class CvApiStub {
  document: NurseCvDocumentDto | undefined = { ...CV };
  getError: unknown = undefined;
  uploadError: unknown = undefined;
  deleteError: unknown = undefined;
  uploaded: File[] = [];
  deleted = 0;

  getCv() {
    if (this.getError !== undefined) {
      return throwError(() => this.getError);
    }
    if (this.document === undefined) {
      return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
    }
    return of(this.document);
  }

  uploadCv(file: File) {
    this.uploaded.push(file);
    if (this.uploadError !== undefined) {
      return throwError(() => this.uploadError);
    }
    this.document = { ...CV, fileName: file.name, fileSizeBytes: file.size };
    return of(this.document);
  }

  deleteCv() {
    this.deleted += 1;
    if (this.deleteError !== undefined) {
      return throwError(() => this.deleteError);
    }
    this.document = undefined;
    return of(undefined);
  }
}

async function setup(api?: CvApiStub): Promise<{ fixture: ComponentFixture<NurseCv>; api: CvApiStub; announcer: Announcer }> {
  const stub = api ?? new CvApiStub();
  TestBed.resetTestingModule();
  await TestBed.configureTestingModule({
    imports: [NurseCv],
    providers: [provideRouter([]), { provide: NurseProfileApi, useValue: stub }],
  }).compileComponents();
  const fixture = TestBed.createComponent(NurseCv);
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  fixture.detectChanges();
  return { fixture, api: stub, announcer: TestBed.inject(Announcer) };
}

function text(fixture: ComponentFixture<NurseCv>): string {
  return fixture.nativeElement.textContent as string;
}

function byTestId(fixture: ComponentFixture<NurseCv>, id: string): HTMLElement | null {
  return fixture.nativeElement.querySelector(`[data-testid="${id}"]`) as HTMLElement | null;
}

async function settle(fixture: ComponentFixture<NurseCv>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await Promise.resolve();
  fixture.detectChanges();
}

describe('NurseCv', () => {
  it('renders no-CV state for GET 404 without a generic error', async () => {
    const api = new CvApiStub();
    api.document = undefined;
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('No CV uploaded yet.');
    expect(text(fixture)).not.toContain('Try again');
  });

  it('renders metadata without exposing contentType, id, or storage internals', async () => {
    const { fixture } = await setup();
    const content = text(fixture);

    expect(content).toContain('nurse-cv.pdf');
    expect(content).toContain('2 KB');
    expect(content).toContain('2026');
    expect(content).not.toContain('application/pdf');
    expect(content).not.toContain('cv-1');
    expect(content).not.toContain('StorageKey');
    expect(content).not.toContain('storageKey');
  });

  it('renders no Preview, Download, or Open UI', async () => {
    const { fixture } = await setup();
    const content = text(fixture).toLowerCase();

    expect(content).not.toContain('preview');
    expect(content).not.toContain('download');
    expect(content).not.toContain('open cv');
    expect(fixture.nativeElement.querySelector('a[href^="blob:"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('iframe')).toBeNull();
  });

  it('requires explicit upload after selection and passes the real File', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      selectFile(file: File): void;
      upload(): Promise<void>;
      selectedFile(): File | undefined;
    };

    component.selectFile(pdfFile());
    fixture.detectChanges();

    expect(api.uploaded.length).toBe(0);
    expect(component.selectedFile()?.name).toBe('nurse-cv.pdf');
    expect(text(fixture)).toContain('nurse-cv.pdf');

    await component.upload();
    await settle(fixture);

    expect(api.uploaded.length).toBe(1);
    expect(api.uploaded[0] instanceof File).toBe(true);
    expect(api.uploaded[0].name).toBe('nurse-cv.pdf');
  });

  it('announces first upload and shows the new metadata', async () => {
    const api = new CvApiStub();
    api.document = undefined;
    const { fixture, announcer } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      selectFile(file: File): void;
      upload(): Promise<void>;
    };

    component.selectFile(pdfFile('first-cv.pdf'));
    await component.upload();
    await settle(fixture);

    expect(text(fixture)).toContain('first-cv.pdf');
    expect(announcer.current()?.text).toBe('CV uploaded.');
  });

  it('announces replacement and shows new metadata without deleting first', async () => {
    const { fixture, api, announcer } = await setup();
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
      upload(): Promise<void>;
    };

    component.startPicking();
    fixture.detectChanges();
    component.selectFile(pdfFile('replacement.pdf'));
    await component.upload();
    await settle(fixture);

    expect(api.uploaded.length).toBe(1);
    expect(api.deleted).toBe(0);
    expect(text(fixture)).toContain('replacement.pdf');
    expect(announcer.current()?.text).toBe('CV replaced.');
  });

  it('preserves existing metadata when replacement upload fails', async () => {
    const api = new CvApiStub();
    api.uploadError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
      upload(): Promise<void>;
    };

    component.startPicking();
    fixture.detectChanges();
    component.selectFile(pdfFile('replacement.pdf'));
    await component.upload();
    await settle(fixture);

    expect(text(fixture)).toContain('nurse-cv.pdf');
    expect(text(fixture)).toContain('could not be uploaded');
  });

  it('blocks zero-byte files', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
    };

    component.startPicking();
    fixture.detectChanges();
    component.selectFile(pdfFile('empty.pdf', 0));
    fixture.detectChanges();

    expect(api.uploaded.length).toBe(0);
    expect(text(fixture)).toContain('must not be empty');
  });

  it('blocks files larger than 5 MiB', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
      upload(): Promise<void>;
    };
    component.startPicking();
    fixture.detectChanges();
    const big = new File([new Uint8Array(5 * 1024 * 1024 + 1)], 'big.pdf', { type: 'application/pdf' });

    component.selectFile(big);
    fixture.detectChanges();
    await component.upload();
    fixture.detectChanges();

    expect(api.uploaded.length).toBe(0);
    expect(text(fixture)).toContain('5 MB or smaller');
  });

  it('blocks unsupported extensions case-insensitively', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
    };

    component.startPicking();
    fixture.detectChanges();
    component.selectFile(new File(['x'], 'notes.txt', { type: 'text/plain' }));
    fixture.detectChanges();

    expect(api.uploaded.length).toBe(0);
    expect(text(fixture)).toContain('not supported');
  });

  it('accepts PDF, DOC, and DOCX regardless of extension casing', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      selectFile(file: File): void;
      upload(): Promise<void>;
    };

    for (const [name, type] of [
      ['cv.PDF', 'application/pdf'],
      ['cv.Doc', 'application/msword'],
      ['cv.DOCX', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
    ] as const) {
      component.selectFile(new File(['x'], name, { type }));
      await component.upload();
      await settle(fixture);
    }

    expect(api.uploaded.length).toBe(3);
  });

  it('blocks unsupported MIME types', async () => {
    const { fixture, api } = await setup();
    const component = fixture.componentInstance as unknown as {
      startPicking(): void;
      selectFile(file: File): void;
    };

    component.startPicking();
    fixture.detectChanges();
    component.selectFile(new File(['x'], 'cv.pdf', { type: 'image/png' }));
    fixture.detectChanges();

    expect(api.uploaded.length).toBe(0);
    expect(text(fixture)).toContain('not supported');
  });

  it('requires a second explicit action before deleting and never deletes on first click', async () => {
    const { fixture, api } = await setup();
    byTestId(fixture, 'delete-cv')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted).toBe(0);
    expect(text(fixture)).toContain('This cannot be undone.');
    expect(byTestId(fixture, 'confirm-delete-cv')).not.toBeNull();
  });

  it('cancels the inline confirmation with Keep without mutating', async () => {
    const { fixture, api } = await setup();
    byTestId(fixture, 'delete-cv')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'keep-cv')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.deleted).toBe(0);
    expect(byTestId(fixture, 'confirm-delete-cv')).toBeNull();
    expect(text(fixture)).toContain('nurse-cv.pdf');
  });

  it('deletes on confirm, returns to no-CV state, and announces the outcome', async () => {
    const { fixture, api, announcer } = await setup();
    byTestId(fixture, 'delete-cv')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'confirm-delete-cv')?.click();
    await settle(fixture);

    expect(api.deleted).toBe(1);
    expect(text(fixture)).toContain('No CV uploaded yet.');
    expect(announcer.current()?.text).toBe('CV deleted.');
  });

  it('treats delete 404 as already removed with a calm notice', async () => {
    const api = new CvApiStub();
    api.deleteError = { status: 404, error: { title: 'Not Found', status: 404 } };
    const { fixture, announcer } = await setup(api);
    byTestId(fixture, 'delete-cv')?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    byTestId(fixture, 'confirm-delete-cv')?.click();
    await settle(fixture);

    expect(text(fixture)).toContain('already been removed');
    expect(announcer.current()?.text).toBe('CV was already removed.');
  });

  it('renders an error and retry affordance on load failure', async () => {
    const api = new CvApiStub();
    api.getError = { status: 500, error: { title: 'Server Error', status: 500 } };
    const { fixture } = await setup(api);

    expect(text(fixture)).toContain('Try again');
    expect(text(fixture)).not.toContain('No CV uploaded yet.');
  });

  it('renders the shared live region for accessible mutation feedback', async () => {
    const { fixture } = await setup();

    expect(fixture.nativeElement.querySelector('np-live-region')).not.toBeNull();
  });
});

describe('Nurse CV route', () => {
  it('mounts /nurse/profile/cv with the three-guard pattern and routeId', () => {
    const route = routes.find((entry) => entry.path === 'nurse/profile/cv');

    expect(route).toBeDefined();
    expect(typeof route?.loadComponent).toBe('function');
    expect(route?.canActivate?.length).toBe(3);
    expect(route?.data).toEqual({ routeId: 'NURSE_PROFILE_CV' });
  });
});
