import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import type { PackageBenefitRightSummaryDto } from '../../../core/api/generated/models/package-benefit-right-summary-dto';

@Component({
  selector: 'np-nurse-entitlement-right',
  imports: [DatePipe],
  templateUrl: './nurse-entitlement-right.html',
  styleUrl: './nurse-entitlement-right.scss',
})
export class NurseEntitlementRight {
  @Input({ required: true }) right!: PackageBenefitRightSummaryDto;

  protected availabilityLabel(): string {
    return this.right.isAvailable ? 'Available' : 'Not available';
  }
}
