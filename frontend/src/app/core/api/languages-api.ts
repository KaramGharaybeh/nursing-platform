import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { listLanguages } from './generated/fn/nursing-platform-web-api/list-languages';
import type { LanguageListItemDto } from './generated/models/language-list-item-dto';

@Injectable({ providedIn: 'root' })
export class LanguagesApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  list(): Observable<readonly LanguageListItemDto[]> {
    return listLanguages(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }
}
