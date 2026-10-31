// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '@cratis/fundamentals';
import { toDate } from './Entitlements/entitlementModel.js';

/**
 * Formats a day for a person.
 * @param day The day.
 * @param locale The locale; the browser's when not given.
 * @returns The day, such as "Nov 2, 2026"; empty when there is no day.
 */
export function formatDay(day: DateOnly | Date | string | undefined, locale?: string): string {
    const date = toDate(day);
    return date ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(date) : '';
}

/**
 * Formats an amount of money.
 * @param amount The amount.
 * @param currency The ISO currency code.
 * @param locale The locale; the browser's when not given.
 * @returns The amount with its currency, such as "$12.00".
 */
export function formatMoney(amount: number, currency: string, locale?: string): string {
    try {
        return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount);
    } catch {
        return `${amount} ${currency}`;
    }
}

/**
 * Formats a quantity.
 * @param value The quantity.
 * @param locale The locale; the browser's when not given.
 * @returns The quantity with its thousands separated.
 */
export function formatQuantity(value: number, locale?: string): string {
    return new Intl.NumberFormat(locale).format(value);
}
