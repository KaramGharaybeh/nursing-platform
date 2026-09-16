import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { getCurrentNurseCv } from './generated/fn/nursing-platform-web-api/get-current-nurse-cv';
import { getCurrentNurseProfile } from './generated/fn/nursing-platform-web-api/get-current-nurse-profile';
import { listCurrentNurseCertificates } from './generated/fn/nursing-platform-web-api/list-current-nurse-certificates';
import { listCurrentNurseEducation } from './generated/fn/nursing-platform-web-api/list-current-nurse-education';
import { listCurrentNurseExperiences } from './generated/fn/nursing-platform-web-api/list-current-nurse-experiences';
import { listCurrentNurseLanguages } from './generated/fn/nursing-platform-web-api/list-current-nurse-languages';
import { listCurrentNurseSkills } from './generated/fn/nursing-platform-web-api/list-current-nurse-skills';
import { upsertCurrentNurseProfile } from './generated/fn/nursing-platform-web-api/upsert-current-nurse-profile';
import type { NurseCertificateDto } from './generated/models/nurse-certificate-dto';
import type { NurseCvDocumentDto } from './generated/models/nurse-cv-document-dto';
import type { NurseEducationDto } from './generated/models/nurse-education-dto';
import type { NurseExperienceDto } from './generated/models/nurse-experience-dto';
import type { NurseLanguageDto } from './generated/models/nurse-language-dto';
import type { NurseProfileDto } from './generated/models/nurse-profile-dto';
import type { NurseSkillDto } from './generated/models/nurse-skill-dto';
import type { UpsertNurseProfileCommand } from './generated/models/upsert-nurse-profile-command';

@Injectable({ providedIn: 'root' })
export class NurseProfileApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  getProfile(): Observable<NurseProfileDto> {
    return getCurrentNurseProfile(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  upsertProfile(request: UpsertNurseProfileCommand): Observable<NurseProfileDto> {
    return upsertCurrentNurseProfile(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }

  listExperiences(): Observable<NurseExperienceDto[]> {
    return listCurrentNurseExperiences(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  listEducation(): Observable<NurseEducationDto[]> {
    return listCurrentNurseEducation(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  listCertificates(): Observable<NurseCertificateDto[]> {
    return listCurrentNurseCertificates(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  listSkills(): Observable<NurseSkillDto[]> {
    return listCurrentNurseSkills(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  listLanguages(): Observable<NurseLanguageDto[]> {
    return listCurrentNurseLanguages(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }

  getCv(): Observable<NurseCvDocumentDto> {
    return getCurrentNurseCv(this.http, this.config.rootUrl).pipe(
      map((response) => response.body),
    );
  }
}
