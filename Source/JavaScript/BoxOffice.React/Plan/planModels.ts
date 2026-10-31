// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { BillingInterval, ModuleAccess, ModuleStanding, Offer, OfferPrice, PortalIntent } from '@cratis/showrunner.boxoffice';
import { Entitlements } from '../Entitlements/entitlementModel.js';
import { formatDay, formatMoney } from '../formatting.js';
import { BoxOfficeLabels, format } from '../Labels/index.js';

/** How a module's access is shown. */
export type AccessSeverity = 'success' | 'info' | 'warn' | 'secondary';

/**
 * What the plan page shows for one module.
 */
export interface ModuleRowModel {
    /** The module's standing. */
    readonly standing: ModuleStanding;

    /** What the access is called. */
    readonly accessText: string;

    /** When the access ends, in words; empty when it does not. */
    readonly untilText: string;

    /** How the access is shown. */
    readonly severity: AccessSeverity;

    /** Whether to offer to try the module. */
    readonly canTry: boolean;
}

/**
 * What the plan page shows for one offer.
 */
export interface OfferRowModel {
    /** The offer. */
    readonly offer: Offer;

    /** The prices, in words, monthly first. */
    readonly prices: readonly string[];

    /** The seats the offer includes, in words; empty when it includes none. */
    readonly seatsText: string;

    /** The names of the modules the offer includes. */
    readonly moduleNames: readonly string[];

    /** What the offer's button opens the customer portal for. */
    readonly intent: PortalIntent;

    /** The text of the offer's button. */
    readonly buttonLabel: string;
}

/**
 * Lays out the modules of the plan page.
 * @param entitlements What the tenant may do.
 * @param labels The labels to show.
 * @param locale The locale to format for; the browser's when not given.
 * @returns A row for every module the product declares.
 */
export function moduleRows(entitlements: Entitlements, labels: BoxOfficeLabels, locale?: string): ModuleRowModel[] {
    return entitlements.modules.map(standing => {
        const access = standing.access;
        return {
            standing,
            accessText: access === ModuleAccess.full
                ? labels.moduleAccess.full
                : access === ModuleAccess.trial
                    ? labels.moduleAccess.trial
                    : access === ModuleAccess.preview ? labels.moduleAccess.preview : labels.moduleAccess.none,
            untilText: standing.until && access !== ModuleAccess.none
                ? format(labels.moduleAccess.until, { date: formatDay(standing.until, locale) })
                : '',
            severity: access === ModuleAccess.full ? 'success' : access === ModuleAccess.trial ? 'info' : access === ModuleAccess.preview ? 'warn' : 'secondary',
            canTry: access === ModuleAccess.none || access === ModuleAccess.preview
        };
    });
}

/**
 * Says what an offer's price is.
 * @param price The price.
 * @param labels The labels to show.
 * @param locale The locale to format for; the browser's when not given.
 * @returns The price, such as "$12.00 per month".
 */
export function priceText(price: OfferPrice, labels: BoxOfficeLabels, locale?: string): string {
    const money = formatMoney(price.amount, price.currency, locale);
    return format(price.interval === BillingInterval.yearly ? labels.plan.perYear : labels.plan.perMonth, { price: money });
}

/**
 * Lays out the offers of the plan page.
 * @param offers The offers on sale.
 * @param entitlements What the tenant may do.
 * @param labels The labels to show.
 * @param locale The locale to format for; the browser's when not given.
 * @returns A row for every offer.
 */
export function offerRows(offers: readonly Offer[], entitlements: Entitlements, labels: BoxOfficeLabels, locale?: string): OfferRowModel[] {
    const changes = entitlements.state === 'licensed';
    return offers.map(offer => ({
        offer,
        prices: [...offer.prices]
            .sort((a, b) => a.interval - b.interval)
            .map(price => priceText(price, labels, locale)),
        seatsText: offer.includedSeats > 0 ? format(labels.plan.includedSeats, { count: offer.includedSeats }) : '',
        moduleNames: offer.modules.map(module => entitlements.standingOf(module)?.name ?? module),
        intent: changes ? PortalIntent.changePlan : PortalIntent.buy,
        buttonLabel: changes ? labels.plan.changePlan : labels.plan.buy
    }));
}

/**
 * Says where the tenant stands with its plan.
 * @param entitlements What the tenant may do.
 * @param labels The labels to show.
 * @returns A sentence; empty when there is nothing to say.
 */
export function planSummary(entitlements: Entitlements, labels: BoxOfficeLabels): string {
    if (entitlements.state === 'trial' && entitlements.trial) {
        return format(labels.plan.onTrial, { days: Math.max(entitlements.trial.daysLeft, 0) });
    }

    return entitlements.seats > 0 ? format(labels.plan.withSeats, { seats: entitlements.seats }) : '';
}
