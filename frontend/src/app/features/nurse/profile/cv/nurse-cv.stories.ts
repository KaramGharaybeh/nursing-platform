import { provideRouter } from '@angular/router';
import { type Meta, type StoryObj } from '@storybook/angular';
import { of, throwError } from 'rxjs';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import { NurseCv } from './nurse-cv';

const DOCUMENT = {
  id: 'cv-1',
  fileName: 'nurse-cv.pdf',
  contentType: 'application/pdf',
  fileSizeBytes: 2048,
  uploadedAt: '2026-09-16T10:00:00Z',
};

class ExistingApi {
  getCv() {
    return of(DOCUMENT);
  }
  uploadCv() {
    return of(DOCUMENT);
  }
  deleteCv() {
    return of(undefined);
  }
}

class EmptyApi extends ExistingApi {
  override getCv() {
    return throwError(() => ({ status: 404, error: { title: 'Not Found', status: 404 } }));
  }
}

function providers(api: ExistingApi) {
  return [provideRouter([]), { provide: NurseProfileApi, useValue: api }];
}

async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

async function openPicker(canvasElement: HTMLElement): Promise<void> {
  await settle();
  const action = canvasElement.querySelector<HTMLElement>(
    '[data-testid="upload-cv"], [data-testid="replace-cv"]',
  );
  if (action === null) {
    throw new Error('CV story could not find the Upload/Replace action.');
  }
  action.click();
  await settle();
}

function chooseFile(canvasElement: HTMLElement, file: File): void {
  const input = canvasElement.querySelector<HTMLInputElement>('[data-testid="cv-file-input"]');
  if (input === null) {
    throw new Error('CV story could not find the file input.');
  }
  const transfer = new DataTransfer();
  transfer.items.add(file);
  input.files = transfer.files;
  input.dispatchEvent(new Event('change', { bubbles: true }));
}

const meta: Meta<NurseCv> = {
  component: NurseCv,
  title: 'Features/Nurse/CV',
};
export default meta;
type Story = StoryObj<NurseCv>;

export const NoCv: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new EmptyApi()) })],
};

export const ExistingCv: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExistingApi()) })],
};

export const UploadReady: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExistingApi()) })],
  play: async ({ canvasElement }) => {
    await openPicker(canvasElement);
    chooseFile(canvasElement, new File(['cv-bytes'], 'replacement.pdf', { type: 'application/pdf' }));
    await settle();
  },
};

export const ValidationError: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExistingApi()) })],
  play: async ({ canvasElement }) => {
    await openPicker(canvasElement);
    chooseFile(canvasElement, new File(['x'], 'notes.txt', { type: 'text/plain' }));
    await settle();
  },
};

export const DeleteConfirmation: Story = {
  decorators: [(story) => ({ ...story(), providers: providers(new ExistingApi()) })],
  play: async ({ canvasElement }) => {
    await settle();
    const remove = canvasElement.querySelector<HTMLElement>('[data-testid="delete-cv"]');
    if (remove === null) {
      throw new Error('CV delete story could not find the Delete action.');
    }
    remove.click();
    await settle();
  },
};
