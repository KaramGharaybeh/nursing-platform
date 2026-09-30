import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { ExamCatalogItem } from '../../core/api/exams-api';
import { buildExamsDetailPath } from '../../core/routing/canonical-routes';
import { LocalizationService } from '../../core/i18n/localization.service';

@Component({
  selector: 'np-exam-card',
  imports: [RouterLink],
  templateUrl: './exam-card.html',
  styleUrl: './exam-card.scss',
})
export class ExamCard {
  readonly exam = input.required<ExamCatalogItem>();
  protected readonly i18n = inject(LocalizationService);

  protected detailPath(): string {
    return buildExamsDetailPath(this.exam().id);
  }

  protected hasDescription(): boolean {
    return (this.exam().description?.trim() ?? '') !== '';
  }

  protected hasCountry(): boolean {
    return this.exam().countryName.trim() !== '';
  }

  protected hasCategory(): boolean {
    return (this.exam().categoryName?.trim() ?? '') !== '';
  }
}
