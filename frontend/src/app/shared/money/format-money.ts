/**
 * Formats a backend minor-unit monetary amount for display.
 *
 * Both inputs remain authoritative from the backend contract: `unitAmountMinor`
 * is an integer minor-unit string and `currency` is a 3-letter currency code.
 * The currency fraction-digit count comes from the runtime ECMA-402 currency
 * metadata — no hand-written currency/divisor table and no assumed decimals.
 * Integer handling uses BigInt internally so arbitrarily large amounts keep
 * exact digits. Malformed amount/currency input throws instead of guessing.
 */
export function formatMoney(
  unitAmountMinor: string,
  currency: string,
  locales?: string | string[],
): string {
  if (typeof unitAmountMinor !== 'string' || typeof currency !== 'string') {
    throw new Error('Cannot format monetary value with the supplied amount and currency.');
  }
  const code = currency.trim().toUpperCase();
  if (!/^[A-Z]{3}$/.test(code)) {
    throw new Error('Cannot format monetary value with the supplied amount and currency.');
  }
  const text = unitAmountMinor.trim();
  if (!/^-?\d+$/.test(text)) {
    throw new Error('Cannot format monetary value with the supplied amount and currency.');
  }
  let formatter: Intl.NumberFormat;
  try {
    formatter = new Intl.NumberFormat(locales, { style: 'currency', currency: code });
  } catch {
    throw new Error('Cannot format monetary value with the supplied amount and currency.');
  }
  const fractionDigits = formatter.resolvedOptions().maximumFractionDigits;
  if (typeof fractionDigits !== 'number') {
    throw new Error('Cannot format monetary value with the supplied amount and currency.');
  }
  const minor = BigInt(text);
  const absolute = minor < 0n ? -minor : minor;
  const divisor = 10n ** BigInt(fractionDigits);
  const major = (absolute / divisor).toString();
  const fraction = (absolute % divisor).toString().padStart(fractionDigits, '0');
  const grouping = new Intl.NumberFormat(formatter.resolvedOptions().locale, {
    useGrouping: true,
  });
  const groupedMajor = grouping.formatToParts(BigInt(major === '' ? '0' : major));
  const template = formatter.formatToParts(minor < 0n ? -1 : 0);
  let output = '';
  let integerReplaced = false;
  for (const part of template) {
    if (part.type === 'integer' || part.type === 'group') {
      if (!integerReplaced) {
        output += groupedMajor.map((grouped) => grouped.value).join('');
        integerReplaced = true;
      }
      continue;
    }
    if (part.type === 'fraction') {
      output += fraction;
      continue;
    }
    output += part.value;
  }
  return output;
}
