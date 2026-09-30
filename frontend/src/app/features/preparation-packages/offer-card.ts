import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { PreparationPackageOfferListItemDto } from '../../core/api/generated/models/preparation-package-offer-list-item-dto';
import { buildPreparationPackageOfferDetailPath } from '../../core/routing/canonical-routes';
import { LocalizationService } from '../../core/i18n/localization.service';

@Component({
  selector: 'np-offer-card',
  imports: [RouterLink],
  templateUrl: './offer-card.html',
  styleUrl: './offer-card.scss',
})
export class OfferCard {
  protected readonly i18n = inject(LocalizationService);
  @Input({ required: true }) offer!: PreparationPackageOfferListItemDto;

  protected detailPath(): string {
    return buildPreparationPackageOfferDetailPath(this.offer.slug);
  }

  protected hasSummary(): boolean {
    return (this.offer.summary?.trim() ?? '') !== '';
  }
}
