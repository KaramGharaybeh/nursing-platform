import { adaptDto, normalizeNullable } from '../api/dto-adapters';
import type { UserDetailDto } from '../api/generated/models/user-detail-dto';

export type CurrentUserStatus = 'idle' | 'loading' | 'ready' | 'anonymous' | 'unavailable';

export interface CurrentUser {
  readonly id: string;
  readonly email: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly isActive: boolean;
  readonly emailVerified: boolean;
  readonly roles: readonly string[];
  readonly permissions: readonly string[];
  readonly createdAt: string;
  readonly lastLoginAt: string | undefined;
}

export function adaptUserDetailToCurrentUser(dto: UserDetailDto): Readonly<CurrentUser> {
  return adaptDto(dto, (source) => ({
    id: source.id,
    email: source.email,
    firstName: source.firstName,
    lastName: source.lastName,
    isActive: source.isActive,
    emailVerified: source.emailVerified,
    roles: Object.freeze([...source.roles]),
    permissions: Object.freeze([...source.permissions]),
    createdAt: source.createdAt,
    lastLoginAt: normalizeNullable(source.lastLoginAt),
  }));
}
