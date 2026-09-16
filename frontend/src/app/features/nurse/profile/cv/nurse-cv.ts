import { DatePipe } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { Announcer, NpLiveRegion } from '../../../../shared/ui/announcement';
import { TwoStepConfirmation } from '../../../../shared/ui/confirmation';
import { NurseProfileApi } from '../../../../core/api/nurse-profile-api';
import type { NurseCvDocumentDto } from '../../../../core/api/generated/models/nurse-cv-document-dto';
import { normalizeProblemDetails } from '../../../../core/api/problem-details';
import type { NormalizedProblemDetails } from '../../../../core/api/problem-details';
import { LoadingErrorRetry } from '../../../../shared/ui/loading-error-retry';
import type { LoadingErrorRetryState } from '../../../../shared/ui/loading-error-retry';

const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024;

const ALLOWED_EXTENSIONS: readonly string[] = ['.pdf', '.doc', '.docx'];

const ALLOWED_CONTENT_TYPES: readonly string[] = [
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
];

export function cvFileExtension(fileName: string): string {
  const dot = fileName.lastIndexOf('.');
  return dot < 0 ? '' : fileName.slice(dot).toLowerCase();
}

export function isAcceptedCvFile(file: File): boolean {
  if (file.size <= 0 || file.size > MAX_FILE_SIZE_BYTES) {
    return false;
  }
  if (!ALLOWED_EXTENSIONS.includes(cvFileExtension(file.name))) {
    return false;
  }
  return ALLOWED_CONTENT_TYPES.some((contentType) => contentType.toLowerCase() === file.type.toLowerCase());
}

@Component({
  selector: 'np-nurse-cv',
  imports: [DatePipe, LoadingErrorRetry, MatButtonModule, NpLiveRegion],
  templateUrl: './nurse-cv.html',
  styleUrl: './nurse-cv.scss',
})
export class NurseCv implements OnInit {
  private readonly api = inject(NurseProfileApi);
  private readonly announcer = inject(Announcer);
  private readonly deleteConfirmation = new TwoStepConfirmation();

  @ViewChild('fileInput') private readonly fileInput?: ElementRef<HTMLInputElement>;

  protected readonly state = signal<LoadingErrorRetryState>({ kind: 'loading' });
  protected readonly document = signal<NurseCvDocumentDto | undefined>(undefined);
  protected readonly picking = signal(false);
  protected readonly selectedFile = signal<File | undefined>(undefined);
  protected readonly selectionError = signal('');
  protected readonly isUploading = signal(false);
  protected readonly uploadError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly isConfirmingDelete = signal(false);
  protected readonly isDeleting = signal(false);
  protected readonly deleteError = signal<NormalizedProblemDetails | undefined>(undefined);
  protected readonly notice = signal('');

  protected get hasDocument(): boolean {
    return this.document() !== undefined;
  }

  protected get uploadLabel(): string {
    return this.hasDocument ? 'Replace CV' : 'Upload CV';
  }

  protected get selectedFileSizeLabel(): string {
    const file = this.selectedFile();
    return file === undefined ? '' : this.formatFileSize(file.size);
  }

  ngOnInit(): void {
    void this.load();
  }

  protected async retry(): Promise<void> {
    await this.load();
  }

  protected startPicking(): void {
    this.picking.set(true);
    this.selectionError.set('');
    this.uploadError.set(undefined);
  }

  protected cancelPicking(): void {
    this.picking.set(false);
    this.clearSelection();
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0];
    this.selectFile(file);
  }

  protected selectFile(file: File | undefined): void {
    this.selectedFile.set(undefined);
    if (file === undefined) {
      this.selectionError.set('');
      return;
    }
    const problem = this.describeFileProblem(file);
    if (problem !== undefined) {
      this.selectionError.set(problem);
      this.resetFileInput();
      return;
    }
    this.selectionError.set('');
    this.selectedFile.set(file);
  }

  protected clearSelection(): void {
    this.selectedFile.set(undefined);
    this.selectionError.set('');
    this.resetFileInput();
  }

  protected async upload(): Promise<void> {
    const file = this.selectedFile();
    if (file === undefined || this.isUploading()) {
      return;
    }
    const hadDocument = this.hasDocument;
    this.isUploading.set(true);
    this.uploadError.set(undefined);
    try {
      const saved = await firstValueFrom(this.api.uploadCv(file));
      this.document.set(saved);
      this.picking.set(false);
      this.clearSelection();
      this.announcer.announce(hadDocument ? 'CV replaced.' : 'CV uploaded.');
    } catch (error: unknown) {
      this.uploadError.set(normalizeProblemDetails(this.errorBody(error)));
    } finally {
      this.isUploading.set(false);
    }
  }

  protected requestDelete(): void {
    this.deleteConfirmation.request();
    this.isConfirmingDelete.set(true);
    this.deleteError.set(undefined);
  }

  protected cancelDelete(): void {
    this.deleteConfirmation.cancel();
    this.isConfirmingDelete.set(false);
    this.deleteError.set(undefined);
  }

  protected async confirmDelete(): Promise<void> {
    if (!this.deleteConfirmation.confirm()) {
      return;
    }
    this.isConfirmingDelete.set(false);
    this.isDeleting.set(true);
    this.deleteError.set(undefined);
    try {
      await firstValueFrom(this.api.deleteCv());
      this.document.set(undefined);
      this.announcer.announce('CV deleted.');
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.document.set(undefined);
        this.notice.set('Your CV had already been removed.');
        this.announcer.announce('CV was already removed.');
      } else {
        this.deleteError.set(normalizeProblemDetails(this.errorBody(error)));
      }
    } finally {
      this.isDeleting.set(false);
    }
  }

  protected formatFileSize(sizeInBytes: number): string {
    if (sizeInBytes >= 1024 * 1024) {
      return `${(sizeInBytes / (1024 * 1024)).toFixed(1)} MB`;
    }
    return `${Math.max(Math.round(sizeInBytes / 1024), 1)} KB`;
  }

  private describeFileProblem(file: File): string | undefined {
    if (file.size <= 0) {
      return 'The selected file must not be empty.';
    }
    if (file.size > MAX_FILE_SIZE_BYTES) {
      return 'The selected file must be 5 MB or smaller.';
    }
    if (!ALLOWED_EXTENSIONS.includes(cvFileExtension(file.name))) {
      return 'This file type is not supported. Choose a PDF, DOC, or DOCX file.';
    }
    const contentType = file.type.toLowerCase();
    if (!ALLOWED_CONTENT_TYPES.some((allowed) => allowed === contentType)) {
      return 'This file type is not supported. Choose a PDF, DOC, or DOCX file.';
    }
    return undefined;
  }

  private resetFileInput(): void {
    if (this.fileInput !== undefined) {
      this.fileInput.nativeElement.value = '';
    }
  }

  private async load(): Promise<void> {
    this.state.set({ kind: 'loading' });
    try {
      this.document.set(await firstValueFrom(this.api.getCv()));
      this.state.set({ kind: 'ready' });
    } catch (error: unknown) {
      if (this.isNotFound(error)) {
        this.document.set(undefined);
        this.state.set({ kind: 'ready' });
        return;
      }
      this.state.set({ kind: 'error', error: normalizeProblemDetails(this.errorBody(error)), canRetry: true });
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && 'status' in error && (error as { status?: unknown }).status === 404;
  }

  private errorBody(error: unknown): unknown {
    if (typeof error !== 'object' || error === null) {
      return error;
    }
    if ('error' in error) {
      return (error as { error?: unknown }).error;
    }
    return error;
  }
}
