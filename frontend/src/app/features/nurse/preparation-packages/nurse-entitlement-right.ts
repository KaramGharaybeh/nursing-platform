import { DatePipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import type { PackageBenefitRightSummaryDto } from '../../../core/api/generated/models/package-benefit-right-summary-dto';
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
  selector: 'np-nurse-entitlement-right',
  imports: [DatePipe],
  templateUrl: './nurse-entitlement-right.html',
  styleUrl: './nurse-entitlement-right.scss',
})
export class NurseEntitlementRight {
  @Input({ required: true }) right!: PackageBenefitRightSummaryDto;
  protected readonly i18n = inject(LocalizationService);

  protected availabilityLabel(): string {
    return this.right.isAvailable ? this.i18n.t('npp.available') : this.i18n.t('npp.notAvailable');
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
