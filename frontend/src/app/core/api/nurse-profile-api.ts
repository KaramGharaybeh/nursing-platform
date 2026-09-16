import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { createNurseCertificate } from './generated/fn/nursing-platform-web-api/create-nurse-certificate';
import { createNurseEducation } from './generated/fn/nursing-platform-web-api/create-nurse-education';
import { createNurseExperience } from './generated/fn/nursing-platform-web-api/create-nurse-experience';
import { deleteNurseCertificate } from './generated/fn/nursing-platform-web-api/delete-nurse-certificate';
import { deleteNurseEducation } from './generated/fn/nursing-platform-web-api/delete-nurse-education';
import { deleteNurseExperience } from './generated/fn/nursing-platform-web-api/delete-nurse-experience';
import { getCurrentNurseCv } from './generated/fn/nursing-platform-web-api/get-current-nurse-cv';
import { deleteNurseCv } from './generated/fn/nursing-platform-web-api/delete-nurse-cv';
import { uploadNurseCv } from './generated/fn/nursing-platform-web-api/upload-nurse-cv';
import { getCurrentNurseProfile } from './generated/fn/nursing-platform-web-api/get-current-nurse-profile';
import { listCurrentNurseCertificates } from './generated/fn/nursing-platform-web-api/list-current-nurse-certificates';
import { listCurrentNurseEducation } from './generated/fn/nursing-platform-web-api/list-current-nurse-education';
import { listCurrentNurseExperiences } from './generated/fn/nursing-platform-web-api/list-current-nurse-experiences';
import { listCurrentNurseLanguages } from './generated/fn/nursing-platform-web-api/list-current-nurse-languages';
import { listCurrentNurseSkills } from './generated/fn/nursing-platform-web-api/list-current-nurse-skills';
import { updateNurseCertificate } from './generated/fn/nursing-platform-web-api/update-nurse-certificate';
import { updateNurseEducation } from './generated/fn/nursing-platform-web-api/update-nurse-education';
import { updateNurseExperience } from './generated/fn/nursing-platform-web-api/update-nurse-experience';
import { updateNurseLanguages } from './generated/fn/nursing-platform-web-api/update-nurse-languages';
import { updateNurseSkills } from './generated/fn/nursing-platform-web-api/update-nurse-skills';
import { upsertCurrentNurseProfile } from './generated/fn/nursing-platform-web-api/upsert-current-nurse-profile';
import type { CreateNurseCertificateCommand } from './generated/models/create-nurse-certificate-command';
import type { CreateNurseEducationCommand } from './generated/models/create-nurse-education-command';
import type { CreateNurseExperienceCommand } from './generated/models/create-nurse-experience-command';
import type { NurseCertificateDto } from './generated/models/nurse-certificate-dto';
import type { NurseCvDocumentDto } from './generated/models/nurse-cv-document-dto';
import type { NurseEducationDto } from './generated/models/nurse-education-dto';
import type { NurseExperienceDto } from './generated/models/nurse-experience-dto';
import type { NurseLanguageDto } from './generated/models/nurse-language-dto';
import type { NurseProfileDto } from './generated/models/nurse-profile-dto';
import type { NurseSkillDto } from './generated/models/nurse-skill-dto';
import type { UpdateNurseCertificateCommand } from './generated/models/update-nurse-certificate-command';
import type { UpdateNurseEducationCommand } from './generated/models/update-nurse-education-command';
import type { UpdateNurseExperienceCommand } from './generated/models/update-nurse-experience-command';
import type { UpdateNurseLanguagesCommand } from './generated/models/update-nurse-languages-command';
import type { UpdateNurseSkillsCommand } from './generated/models/update-nurse-skills-command';
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

  createExperience(request: CreateNurseExperienceCommand): Observable<NurseExperienceDto> {
    return createNurseExperience(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }

  updateExperience(id: string, request: Omit<UpdateNurseExperienceCommand, 'id'>): Observable<NurseExperienceDto> {
    return updateNurseExperience(this.http, this.config.rootUrl, { id, body: { ...request, id } }).pipe(
      map((response) => response.body),
    );
  }

  deleteExperience(id: string): Observable<void> {
    return deleteNurseExperience(this.http, this.config.rootUrl, { id }).pipe(map(() => undefined));
  }

  createEducation(request: CreateNurseEducationCommand): Observable<NurseEducationDto> {
    return createNurseEducation(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }

  updateEducation(id: string, request: Omit<UpdateNurseEducationCommand, 'id'>): Observable<NurseEducationDto> {
    return updateNurseEducation(this.http, this.config.rootUrl, { id, body: { ...request, id } }).pipe(
      map((response) => response.body),
    );
  }

  deleteEducation(id: string): Observable<void> {
    return deleteNurseEducation(this.http, this.config.rootUrl, { id }).pipe(map(() => undefined));
  }

  createCertificate(request: CreateNurseCertificateCommand): Observable<NurseCertificateDto> {
    return createNurseCertificate(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }

  updateCertificate(id: string, request: Omit<UpdateNurseCertificateCommand, 'id'>): Observable<NurseCertificateDto> {
    return updateNurseCertificate(this.http, this.config.rootUrl, { id, body: { ...request, id } }).pipe(
      map((response) => response.body),
    );
  }

  deleteCertificate(id: string): Observable<void> {
    return deleteNurseCertificate(this.http, this.config.rootUrl, { id }).pipe(map(() => undefined));
  }

  updateSkills(request: UpdateNurseSkillsCommand): Observable<NurseSkillDto[]> {
    return updateNurseSkills(this.http, this.config.rootUrl, { body: request }).pipe(
      map((response) => response.body),
    );
  }

  updateLanguages(request: UpdateNurseLanguagesCommand): Observable<NurseLanguageDto[]> {
    return updateNurseLanguages(this.http, this.config.rootUrl, { body: request }).pipe(
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

  /**
   * T-FE-037 approved multipart-upload pattern: the real browser File flows
   * straight into the generated multipart operation, which owns FormData
   * construction and preserves File.name in the part disposition.
   */
  uploadCv(file: File): Observable<NurseCvDocumentDto> {
    return uploadNurseCv(this.http, this.config.rootUrl, { body: { file } }).pipe(
      map((response) => response.body),
    );
  }

  deleteCv(): Observable<void> {
    return deleteNurseCv(this.http, this.config.rootUrl).pipe(map(() => undefined));
  }
}
