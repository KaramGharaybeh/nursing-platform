export function normalizeNullable<T>(value: T | null | undefined): T | undefined {
  if (value === null || value === undefined) {
    return undefined;
  }
  return value;
}

export function adaptDto<TSource, TResult>(
  source: TSource,
  mapper: (source: TSource) => TResult,
): Readonly<TResult> {
  const mapped = mapper(source);
  return Object.freeze(mapped) as Readonly<TResult>;
}

export interface PaginatedSource<TItem> {
  readonly items: readonly TItem[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface PaginatedView<TItem> {
  readonly items: readonly TItem[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export function adaptPaginatedResult<TSourceItem, TResultItem>(
  source: PaginatedSource<TSourceItem>,
  mapper: (item: TSourceItem) => TResultItem,
): Readonly<PaginatedView<TResultItem>> {
  const items = Object.freeze(source.items.map((item) => adaptDto(item, mapper)));
  return Object.freeze({
    items,
    page: source.page,
    pageSize: source.pageSize,
    totalCount: source.totalCount,
    totalPages: source.totalPages,
  });
}
