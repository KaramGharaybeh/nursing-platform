import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TwoStepConfirmation } from '../../../shared/ui/confirmation';
import type { ReceivedContactRequestDto } from '../../../core/api/generated/models/received-contact-request-dto';

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

  protected get isPending(): boolean {
    return this.request.status === 'Pending';
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
