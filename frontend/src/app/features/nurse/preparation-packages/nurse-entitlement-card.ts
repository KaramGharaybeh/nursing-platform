import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { PackageEntitlementListItemDto } from '../../../core/api/generated/models/package-entitlement-list-item-dto';
import { buildPreparationPackageEntitlementDetailPath } from '../../../core/routing/canonical-routes';

@Component({
  selector: 'np-nurse-entitlement-card',
  imports: [DatePipe, RouterLink],
  templateUrl: './nurse-entitlement-card.html',
  styleUrl: './nurse-entitlement-card.scss',
})
export class NurseEntitlementCard {
  @Input({ required: true }) entitlement!: PackageEntitlementListItemDto;

  protected detailPath(): string {
    return buildPreparationPackageEntitlementDetailPath(this.entitlement.id);
  }

  protected availabilityLabel(isAvailable: boolean): string {
    return isAvailable ? 'Available' : 'Not available';
  }
}
