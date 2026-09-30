import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import { firstValueFrom } from 'rxjs';
import { provideApiConfig } from './api-config';
import { NurseProfileApi } from './nurse-profile-api';
import type { NurseCvDocumentDto } from './generated/models/nurse-cv-document-dto';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

/**
 * T-FE-037 proving evidence: the approved frontend multipart-upload pattern is
 * real browser File → facade → generated uploadNurseCv({ body: { file } }) →
 * generated RequestBuilder multipart encoding. No shared helper is extracted:
 * the generated client already owns multipart construction and CV upload is
 * the only consumer.
 */
describe('nurse-profile-api multipart upload (T-FE-037)', () => {
  let api: NurseProfileApi;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideApiConfig()],
    });

    api = TestBed.inject(NurseProfileApi);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('uploads a real File through the generated multipart operation preserving its name', async () => {
    const file = new File(['cv-bytes'], 'nurse-cv.pdf', { type: 'application/pdf' });
    const expected: NurseCvDocumentDto = {
      id: 'cv-1',
      fileName: 'nurse-cv.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 8,
      uploadedAt: '2026-09-16T10:00:00Z',
    };

    const result = firstValueFrom(api.uploadCv(file));
    const request = httpMock.expectOne('/api/v1/me/nurse-profile/cv');

    expect(request.request.method).toBe('POST');
    const body = request.request.body as FormData;
    expect(body instanceof FormData).toBe(true);
    const part = body.get('file');
    expect(part instanceof File).toBe(true);
    expect(part).toBe(file);
    expect((part as File).name).toBe('nurse-cv.pdf');

    request.flush(expected);

    await expect(result).resolves.toEqual(expected);
  });

  it('does not construct multipart bodies by hand outside the generated client', () => {
    const source = readFileSync(join(nodeGlobal.process.cwd(), 'src/app/core/api/nurse-profile-api.ts'), 'utf8');

    expect(source).not.toContain('new FormData');
    expect(source).not.toContain('MultipartHelper');
    expect(source).not.toContain('FileUploadService');
    expect(source).toContain('uploadNurseCv');
  });
});
