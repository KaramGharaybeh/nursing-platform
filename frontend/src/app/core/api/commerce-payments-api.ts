import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiConfiguration } from './generated/api-configuration';
import { createMyPaymentOrder } from './generated/fn/nursing-platform-web-api/create-my-payment-order';
import { getPaymentProduct } from './generated/fn/nursing-platform-web-api/get-payment-product';
import { listPaymentProducts } from './generated/fn/nursing-platform-web-api/list-payment-products';

const PRODUCT_PAGE_SIZE = 20;

export interface CommerceProduct {
  readonly id: string;
  readonly name: string;
  readonly description: string | null;
  readonly examTitle: string | null;
  readonly currency: string;
  readonly unitAmountMinor: string;
  readonly isActive: boolean;
}

export interface CommerceProductPage {
  readonly items: CommerceProduct[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface CommerceOrderSource {
  readonly productId?: string;
  readonly packageOfferId?: string;
}

export interface CommerceOrderItem {
  readonly title: string;
  readonly currency: string;
  readonly lineTotalAmountMinor: string;
}

export interface CommerceOrder {
  readonly id: string;
  readonly status: string;
  readonly currency: string;
  readonly totalAmountMinor: string;
  readonly createdAt: string;
  readonly items: readonly CommerceOrderItem[];
}

@Injectable({ providedIn: 'root' })
export class CommercePaymentsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ApiConfiguration);

  listProducts(page: number): Observable<CommerceProductPage> {
    return listPaymentProducts(this.http, this.config.rootUrl, {
      page,
      pageSize: PRODUCT_PAGE_SIZE,
    }).pipe(map((response) => adaptProductPage(response.body)));
  }

  getProduct(productId: string): Observable<CommerceProduct> {
    return getPaymentProduct(this.http, this.config.rootUrl, { id: productId }).pipe(
      map((response) => adaptProduct(response.body)),
    );
  }

  createOrder(source: CommerceOrderSource): Observable<CommerceOrder> {
    const productId = asNullableString(source.productId);
    const packageOfferId = asNullableString(source.packageOfferId);
    const hasProduct = productId !== null && productId.trim() !== '';
    const hasPackageOffer = packageOfferId !== null && packageOfferId.trim() !== '';
    if (hasProduct === hasPackageOffer) {
      throw new Error('Order creation requires exactly one purchase source.');
    }
    const body = hasProduct ? { productId } : { packageOfferId };
    return createMyPaymentOrder(this.http, this.config.rootUrl, { body }).pipe(
      map((response) => adaptOrder(response.body)),
    );
  }
}

function adaptProductPage(body: unknown): CommerceProductPage {
  const page = asRecord(body);
  const items = Array.isArray(page['items']) ? page['items'].map(adaptProduct) : [];
  return {
    items,
    page: asNumber(page['page'], 0),
    pageSize: asNumber(page['pageSize'], 0),
    totalCount: asNumber(page['totalCount'], 0),
    totalPages: asNumber(page['totalPages'], 0),
  };
}

function adaptProduct(item: unknown): CommerceProduct {
  const record = asRecord(item);
  const id = asString(record['id']);
  const name = asString(record['name']);
  const currency = asString(record['currency']);
  const unitAmountMinor = asString(record['unitAmountMinor']);
  const description = asNullableString(record['description']);
  const examTitle = asNullableString(record['examTitle']);
  if (
    id === '' ||
    name.trim() === '' ||
    !/^[A-Za-z]{3}$/.test(currency.trim()) ||
    !/^-?\d+$/.test(unitAmountMinor.trim())
  ) {
    throw new Error('Product response did not include usable product content.');
  }
  return {
    id,
    name,
    description: description === null || description.trim() === '' ? null : description,
    examTitle: examTitle === null || examTitle.trim() === '' ? null : examTitle,
    currency,
    unitAmountMinor,
    isActive: record['isActive'] === true,
  };
}

function asRecord(value: unknown): Record<string, unknown> {
  return typeof value === 'object' && value !== null
    ? (value as Record<string, unknown>)
    : {};
}

function asString(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

function asNullableString(value: unknown): string | null {
  return typeof value === 'string' ? value : null;
}

function asNumber(value: unknown, fallback: number): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : fallback;
}

function adaptOrder(body: unknown): CommerceOrder {
  const order = asRecord(body);
  const id = asString(order['id']);
  const status = asString(order['status']);
  const currency = asString(order['currency']);
  const totalAmountMinor = asString(order['totalAmountMinor']);
  const createdAt = asString(order['createdAt']);
  const items = Array.isArray(order['items']) ? order['items'].map(adaptOrderItem) : [];
  if (
    id === '' ||
    status.trim() === '' ||
    !/^[A-Za-z]{3}$/.test(currency.trim()) ||
    !/^-?\d+$/.test(totalAmountMinor.trim()) ||
    createdAt.trim() === '' ||
    items.length === 0
  ) {
    throw new Error('Order response did not include usable order content.');
  }
  return { id, status, currency, totalAmountMinor, createdAt, items };
}

function adaptOrderItem(item: unknown): CommerceOrderItem {
  const record = asRecord(item);
  const title = asString(record['productName']);
  const currency = asString(record['currency']);
  const lineTotalAmountMinor = asString(record['lineTotalAmountMinor']);
  if (
    title.trim() === '' ||
    !/^[A-Za-z]{3}$/.test(currency.trim()) ||
    !/^-?\d+$/.test(lineTotalAmountMinor.trim())
  ) {
    throw new Error('Order response did not include usable order content.');
  }
  return { title, currency, lineTotalAmountMinor };
}
