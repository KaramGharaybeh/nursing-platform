import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { listCountries } from './generated/fn/nursing-platform-web-api/list-countries';
import type { CountryListItemDto } from './generated/models/country-list-item-dto';

@Injectable({ providedIn: 'root' })
export class CountriesApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  list(): Observable<readonly CountryListItemDto[]> {
    return listCountries(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }
}
