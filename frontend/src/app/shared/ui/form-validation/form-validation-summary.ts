import { Component, ElementRef, Input, inject } from '@angular/core';
import type { NpValidationSummaryItem } from './form-validation';

let nextSummaryId = 0;

@Component({
  selector: 'np-form-validation-summary',
  templateUrl: './form-validation-summary.html',
  styleUrl: './form-validation-summary.scss',
})
export class NpFormValidationSummary {
  @Input() summaryTitle = 'Check the highlighted fields';
  @Input() items: readonly NpValidationSummaryItem[] = [];
  @Input() formErrors: readonly string[] = [];
  @Input() summaryId = `np-form-validation-summary-${++nextSummaryId}`;

  private readonly host = inject(ElementRef);

  protected get hasSummary(): boolean {
    return this.items.length > 0 || this.formErrors.length > 0;
  }

  protected get titleId(): string {
    return `${this.summaryId}-title`;
  }

  focusSummary(): void {
    const section = this.host.nativeElement.querySelector('section') as HTMLElement | null;
    section?.focus();
  }
}
