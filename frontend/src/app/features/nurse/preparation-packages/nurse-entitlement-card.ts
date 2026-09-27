import { DatePipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { PackageEntitlementListItemDto } from '../../../core/api/generated/models/package-entitlement-list-item-dto';
import { buildPreparationPackageEntitlementDetailPath } from '../../../core/routing/canonical-routes';
import { LocalizationService } from '../../../core/i18n/localization.service';
import type { TranslationKey } from '../../../core/i18n/translations';

const ENTITLEMENT_STATUS_KEYS: Record<string, TranslationKey> = {
  Active: 'npp.statusActive',
  Expired: 'npp.statusExpired',
  Locked: 'npp.statusLocked',
};

const RIGHT_TYPE_KEYS: Record<string, TranslationKey> = {
  StudyMaterial: 'npp.rightStudyMaterial',
  Practice: 'npp.rightPractice',
  PracticeAccess: 'npp.rightPracticeAccess',
  MaterialsAccess: 'npp.rightMaterialsAccess',
  PackageExamAttemptEligibility: 'npp.rightExamAttempt',
  ExamAttempt: 'npp.rightExamAttemptShort',
  ReportEligibility: 'npp.rightReport',
};

@Component({
  selector: 'np-nurse-entitlement-card',
  imports: [DatePipe, RouterLink],
  templateUrl: './nurse-entitlement-card.html',
  styleUrl: './nurse-entitlement-card.scss',
})
export class NurseEntitlementCard {
  @Input({ required: true }) entitlement!: PackageEntitlementListItemDto;
  protected readonly i18n = inject(LocalizationService);

  protected detailPath(): string {
    return buildPreparationPackageEntitlementDetailPath(this.entitlement.id);
  }

  protected availabilityLabel(isAvailable: boolean): string {
    return isAvailable ? this.i18n.t('npp.available') : this.i18n.t('npp.notAvailable');
  }

  protected statusText(status: string): string {
    const key = ENTITLEMENT_STATUS_KEYS[status];
    return key === undefined ? status : this.i18n.t(key);
  }

  protected rightTypeText(rightType: string): string {
    const key = RIGHT_TYPE_KEYS[rightType];
    return key === undefined ? rightType : this.i18n.t(key);
  }
}
