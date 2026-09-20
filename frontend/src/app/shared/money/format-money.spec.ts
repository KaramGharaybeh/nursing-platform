import { formatMoney } from './format-money';

function digits(value: string): string {
  return value.replace(/[^0-9]/g, '');
}

describe('formatMoney (T-FE-082)', () => {
  it('formats a 2-decimal currency without assuming punctuation', () => {
    const formatted = formatMoney('4999', 'USD', 'en-US');

    expect(formatted).toBe('$49.99');
    expect(digits(formatMoney('4999', 'USD'))).toBe('4999');
  });

  it('formats a 0-decimal currency without inventing fraction digits', () => {
    const formatted = formatMoney('500', 'JPY', 'en-US');

    expect(digits(formatted)).toBe('500');
    expect(formatted).not.toMatch(/5[.,]00/);
  });

  it('formats a 3-decimal currency with exact minor digits', () => {
    const formatted = formatMoney('1234', 'BHD', 'en-US');

    expect(digits(formatted)).toBe('1234');
    expect(formatted).toContain('1.234');
  });

  it('formats zero amounts exactly', () => {
    expect(formatMoney('0', 'USD', 'en-US')).toBe('$0.00');
    expect(digits(formatMoney('0', 'JPY'))).toBe('0');
  });

  it('formats large integer amounts without precision loss', () => {
    const formatted = formatMoney('99999999999999999999', 'USD', 'en-US');

    expect(digits(formatted)).toBe('99999999999999999999');
    expect(formatted.endsWith('.99')).toBe(true);
  });

  it('uses the runtime locale by default and stays readable', () => {
    const formatted = formatMoney('2500', 'EUR');

    expect(digits(formatted)).toBe('2500');
    expect(formatted.length).toBeGreaterThan(0);
  });

  it('rejects malformed minor-unit strings without guessing', () => {
    for (const bad of ['', '   ', '12.5', 'abc', '1,000', '0x10', '+5', '--5']) {
      expect(() => formatMoney(bad, 'USD', 'en-US')).toThrow();
    }
  });

  it('rejects malformed currencies without guessing', () => {
    for (const bad of ['', 'US', 'USDD', '12', 'U$D', '   ']) {
      expect(() => formatMoney('100', bad, 'en-US')).toThrow();
    }
  });

  it('accepts lowercase currency codes by normalizing case', () => {
    expect(formatMoney('100', 'usd', 'en-US')).toBe('$1.00');
  });
});
