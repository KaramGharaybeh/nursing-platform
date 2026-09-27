import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TwoStepConfirmation } from '../../../shared/ui/confirmation';
import { LocalizationService } from '../../../core/i18n/localization.service';
import type { TranslationKey } from '../../../core/i18n/translations';
import type { ReceivedContactRequestDto } from '../../../core/api/generated/models/received-contact-request-dto';

const REQUEST_STATUS_KEYS: Record<string, TranslationKey> = {
  Pending: 'contact.statusPending',
  Approved: 'contact.statusApproved',
  Rejected: 'contact.statusRejected',
  Cancelled: 'contact.statusCancelled',
};

@Component({
  selector: 'np-nurse-contact-request-card',
  imports: [DatePipe, MatButtonModule],
  templateUrl: './nurse-contact-request-card.html',
  styleUrl: './nurse-contact-request-card.scss',
})
export class NurseContactRequestCard {
  @Input({ required: true }) request!: ReceivedContactRequestDto;
  @Input() isMutating = false;
  @Input() mutationError = '';

  @Output() readonly approveRequested = new EventEmitter<string>();
  @Output() readonly rejectConfirmed = new EventEmitter<string>();

  private readonly rejectConfirmation = new TwoStepConfirmation();
  protected readonly i18n = inject(LocalizationService);

  protected get isPending(): boolean {
    return this.request.status === 'Pending';
  }

  protected get statusText(): string {
    const key = REQUEST_STATUS_KEYS[this.request.status];
    return key === undefined ? this.request.status : this.i18n.t(key);
  }

  protected get isConfirmingReject(): boolean {
    return this.rejectConfirmation.canExecute;
  }

  protected get contextLine(): string {
    const parts = [this.request.jobTitle, this.request.department]
      .map((part) => part?.trim() ?? '')
      .filter((part) => part !== '');
    return parts.join(' · ');
  }

  protected approve(): void {
    if (!this.isMutating) {
      this.approveRequested.emit(this.request.id);
    }
  }

  protected requestReject(): void {
    this.rejectConfirmation.request();
  }

  protected cancelReject(): void {
    this.rejectConfirmation.cancel();
  }

  protected confirmReject(): void {
    if (this.rejectConfirmation.confirm()) {
      this.rejectConfirmed.emit(this.request.id);
    }
  }
}
