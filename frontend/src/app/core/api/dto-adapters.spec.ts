import type {
  PaginatedResultOfPreparationPackageOfferListItemDto,
  PaymentOrderDto,
  PreparationPackageOfferListItemDto,
} from './generated/models';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { readFileSync } from 'node:fs';
// @ts-expect-error - Vitest runs in Node; node builtins resolve at runtime.
import { join } from 'node:path';
import {
  adaptDto,
  adaptPaginatedResult,
  normalizeNullable,
} from './dto-adapters';

const nodeGlobal = globalThis as unknown as { process: { cwd(): string } };

function readTextFile(relativePath: string): string {
  return readFileSync(join(nodeGlobal.process.cwd(), relativePath), 'utf8');
}

interface OfferView {
  readonly id: string;
  readonly title: string;
  readonly summary: string | undefined;
}

function offerSource(): PreparationPackageOfferListItemDto {
  return {
    accessDurationDays: 30,
    countryId: 'country-1',
    countryName: 'Country',
    currency: 'USD',
    examCategoryId: 'category-1',
    examCategoryName: 'Category',
    examId: 'exam-1',
    examTitle: 'Exam',
    id: 'offer-1',
    materialCount: 2,
    practiceItemCount: 5,
    priceAmountMinor: '1000',
    slug: 'offer-1',
    summary: 'Offer summary',
    title: 'Offer title',
  };
}

function toOfferView(source: PreparationPackageOfferListItemDto): OfferView {
  return {
    id: source.id,
    title: source.title,
    summary: normalizeNullable(source.summary),
  };
}

describe('dto-adapters', () => {
  it('maps a generated DTO to a frontend-owned shape through an explicit mapper', () => {
    const result = adaptDto(offerSource(), toOfferView);

    expect(result).toEqual({
      id: 'offer-1',
      title: 'Offer title',
      summary: 'Offer summary',
    });
  });

  it('does not leak unknown or extra generated fields into the adapted output', () => {
    const sourceWithExtra = {
      ...offerSource(),
      unknownFutureField: 'must-not-leak',
      nested: { leaked: true },
    } as PreparationPackageOfferListItemDto & Record<string, unknown>;

    const result = adaptDto(sourceWithExtra, toOfferView);

    expect(result).toEqual({
      id: 'offer-1',
      title: 'Offer title',
      summary: 'Offer summary',
    });
    expect('unknownFutureField' in result).toBe(false);
    expect('nested' in result).toBe(false);
    expect(JSON.stringify(result)).not.toContain('must-not-leak');
  });

  it('does not mutate the source DTO and returns a frozen result', () => {
    const source = offerSource();
    const snapshot = JSON.stringify(source);

    const result = adaptDto(source, toOfferView);

    expect(JSON.stringify(source)).toBe(snapshot);
    expect(Object.isFrozen(result)).toBe(true);
  });

  it('normalizes null and undefined deterministically to undefined', () => {
    expect(normalizeNullable<string>('value')).toBe('value');
    expect(normalizeNullable<string>(null)).toBeUndefined();
    expect(normalizeNullable<string>(undefined)).toBeUndefined();
    expect(normalizeNullable<number>(0)).toBe(0);
  });

  it('maps optional nullable generated fields to undefined without inventing defaults', () => {
    const withNull = adaptDto(
      { ...offerSource(), summary: null },
      toOfferView,
    );
    const withMissing = adaptDto(
      { ...offerSource(), summary: undefined },
      toOfferView,
    );

    expect(withNull.summary).toBeUndefined();
    expect(withMissing.summary).toBeUndefined();
    expect('summary' in withNull ? withNull.summary : undefined).toBeUndefined();
  });

  it('preserves pagination metadata verbatim while mapping items', () => {
    const source: PaginatedResultOfPreparationPackageOfferListItemDto = {
      items: [
        offerSource(),
        { ...offerSource(), id: 'offer-2', title: 'Second offer' },
      ],
      page: 2,
      pageSize: 2,
      totalCount: 5,
      totalPages: 3,
    };
    const snapshot = JSON.stringify(source);

    const result = adaptPaginatedResult(source, toOfferView);

    expect(result.page).toBe(2);
    expect(result.pageSize).toBe(2);
    expect(result.totalCount).toBe(5);
    expect(result.totalPages).toBe(3);
    expect(result.items).toHaveLength(2);
    expect(result.items[0]).toEqual({
      id: 'offer-1',
      title: 'Offer title',
      summary: 'Offer summary',
    });
    expect(result.items[1]).toEqual({
      id: 'offer-2',
      title: 'Second offer',
      summary: 'Offer summary',
    });
    expect(JSON.stringify(source)).toBe(snapshot);
    expect(Object.isFrozen(result)).toBe(true);
    expect(Object.isFrozen(result.items)).toBe(true);
  });

  it('preserves generated enum and date transport values verbatim without reinterpretation', () => {
    const source: PaymentOrderDto = {
      createdAt: '2026-09-03T10:00:00Z',
      currency: 'USD',
      id: 'order-1',
      items: [],
      status: 'PendingPayment',
      totalAmountMinor: '1000',
      updatedAt: '2026-09-03T10:00:00Z',
    };

    const result = adaptDto(source, (value) => ({
      id: value.id,
      status: value.status,
      createdAt: value.createdAt,
    }));

    expect(result.status).toBe('PendingPayment');
    expect(result.createdAt).toBe('2026-09-03T10:00:00Z');
  });

  it('keeps Problem Details handling separate from the DTO adapter boundary', () => {
    const source = readTextFile('src/app/core/api/dto-adapters.ts').toLowerCase();

    expect(source).not.toContain('problemdetails');
    expect(source).not.toContain('normalizeproblemdetails');
    expect(source).not.toContain('traceid');
    expect(source).not.toContain('retryafterseconds');
  });

  it('does not depend on generated implementation internals beyond exported DTO shapes', () => {
    const source = readTextFile('src/app/core/api/dto-adapters.ts');

    expect(source).not.toContain('RequestBuilder');
    expect(source).not.toContain('ApiConfiguration');
    expect(source).not.toContain('HttpClient');
    expect(source).not.toContain('generated/fn/');
    expect(source).not.toContain('generated/services/');
  });

  it('keeps the boundary free of auth, session, routing, UI, and feature behavior', () => {
    const source = readTextFile('src/app/core/api/dto-adapters.ts').toLowerCase();

    expect(source).not.toContain('httpclient');
    expect(source).not.toContain('localstorage');
    expect(source).not.toContain('sessionstorage');
    expect(source).not.toContain('router');
    expect(source).not.toContain('navigate');
    expect(source).not.toContain('snackbar');
    expect(source).not.toContain('toast');
    expect(source).not.toContain('token');
    expect(source).not.toContain('login');
  });
});
