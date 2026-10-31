// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '@cratis/fundamentals';
import {
    AllowancePeriod,
    BillingInterval,
    EntitlementSummary,
    MeterStanding,
    ModuleAccess,
    ModuleStanding,
    Offer,
    OfferPrice,
    OverageBehavior,
    Person,
    ReadOnlyReason,
    SeatHolding,
    SeatRoster,
    SeatStanding,
    SeatStatus
} from '@cratis/showrunner.boxoffice';
import { Guid } from '@cratis/fundamentals';
import { Entitlements, entitlementsOf } from '../Entitlements/entitlementModel.js';

/** The day it is in the specifications and the stories. */
export const today = new Date(2026, 10, 2, 12, 0, 0);

/**
 * Makes a day relative to {@link today}.
 * @param days How many days after today; negative for before.
 * @returns The day.
 */
export const dayFromToday = (days: number): DateOnly => DateOnly.fromDate(new Date(2026, 10, 2 + days));

/**
 * Makes what a module's standing is.
 * @param module The module.
 * @param name What it is called.
 * @param access The access.
 * @param until The last day of the access, in days from today.
 * @returns The {@link ModuleStanding}.
 */
export function moduleStanding(module: string, name: string, access: ModuleAccess, until?: number): ModuleStanding {
    return Object.assign(new ModuleStanding(), { module, name, access, until: until === undefined ? undefined : dayFromToday(until) });
}

/** The modules of the sample product. */
export const sampleModules = (): ModuleStanding[] => [
    moduleStanding('core', 'Core', ModuleAccess.full),
    moduleStanding('analytics', 'Analytics', ModuleAccess.trial, 9),
    moduleStanding('agents', 'Agents', ModuleAccess.none)
];

/**
 * Makes an entitlement summary.
 * @param overrides What differs from a licensed tenant with ten seats.
 * @returns The {@link EntitlementSummary}.
 */
export function summaryOf(overrides: Partial<EntitlementSummary> = {}): EntitlementSummary {
    return Object.assign(new EntitlementSummary(), {
        readOnlyReason: ReadOnlyReason.none,
        isTrial: false,
        trialEnds: undefined,
        seats: 10,
        seatsUntil: undefined,
        modules: sampleModules(),
        canOpenPortal: true
    }, overrides);
}

/** A tenant on a trial with some days left. */
export const trialSummary = (daysLeft = 12): EntitlementSummary => summaryOf({ isTrial: true, trialEnds: dayFromToday(daysLeft), seats: 5 });

/** A tenant whose trial ended without seats. */
export const closedTrialSummary = (): EntitlementSummary =>
    summaryOf({ isTrial: true, trialEnds: dayFromToday(-3), seats: 0, readOnlyReason: ReadOnlyReason.trialClosed });

/** A tenant the operator suspended. */
export const suspendedSummary = (): EntitlementSummary => summaryOf({ readOnlyReason: ReadOnlyReason.tenantSuspended });

/**
 * Works out the entitlements of a summary on {@link today}.
 * @param summary The summary.
 * @returns The {@link Entitlements}.
 */
export const entitlementsFor = (summary: EntitlementSummary | undefined): Entitlements => entitlementsOf(summary, today);

/**
 * Makes a person.
 * @param subject The subject at the identity provider.
 * @param name The name.
 * @param email The email address.
 * @returns The {@link Person}.
 */
export const person = (subject: string, name: string, email: string): Person =>
    Object.assign(new Person(), { identityProvider: 'github', identityProviderSubject: subject, name, email });

/**
 * Makes a seat.
 * @param holder The person who holds it.
 * @param daysAgo How many days ago it was assigned.
 * @returns The {@link SeatHolding}.
 */
export const holding = (holder: Person, daysAgo: number): SeatHolding =>
    Object.assign(new SeatHolding(), { holder, assignedAt: new Date(2026, 10, 2 - daysAgo, 9, 0, 0) });

/** The people of the sample tenant. */
export const ada = person('1001', 'Ada Lovelace', 'ada@fjord.example.com');
export const bo = person('1002', 'Bo Nilsen', 'bo@fjord.example.com');
export const cy = person('1003', 'Cy Hansen', 'cy@fjord.example.com');

/**
 * Makes a seat roster.
 * @param holdings The seats.
 * @returns The {@link SeatRoster}.
 */
export const rosterOf = (...holdings: SeatHolding[]): SeatRoster =>
    Object.assign(new SeatRoster(), { id: 'fjord', holdings, held: holdings.length });

/**
 * Makes where a user stands with the seats.
 * @param status The status.
 * @param capacity How many seats the tenant has.
 * @param held How many are held.
 * @returns The {@link SeatStanding}.
 */
export const seatStanding = (status: SeatStatus, capacity = 10, held = 3): SeatStanding =>
    Object.assign(new SeatStanding(), { status, capacity, held, readOnlyReason: ReadOnlyReason.none });

/**
 * Makes how a meter stands.
 * @param overrides What differs from a meter at 40% of a monthly allowance.
 * @returns The {@link MeterStanding}.
 */
export const meterStanding = (overrides: Partial<MeterStanding> = {}): MeterStanding =>
    Object.assign(new MeterStanding(), {
        meter: 'agent-runs',
        name: 'Agent runs',
        unit: 'runs',
        used: 400,
        allowance: 1000,
        period: AllowancePeriod.month,
        overage: OverageBehavior.block,
        periodEnds: dayFromToday(29),
        budget: undefined,
        concurrencyLimit: undefined,
        hasConcurrencyLimit: false
    }, overrides);

/**
 * Makes an offer.
 * @param name What it is called.
 * @param modules The modules it includes.
 * @param includedSeats The seats it includes.
 * @param monthly The monthly price.
 * @returns The {@link Offer}.
 */
export function offerOf(name: string, modules: string[], includedSeats: number, monthly: number): Offer {
    const month = Object.assign(new OfferPrice(), { interval: BillingInterval.monthly, amount: monthly, currency: 'USD' });
    const year = Object.assign(new OfferPrice(), { interval: BillingInterval.yearly, amount: monthly * 10, currency: 'USD' });
    return Object.assign(new Offer(), { id: Guid.create(), name, modules, includedSeats, prices: [year, month], withdrawn: false });
}
