// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '@cratis/fundamentals';
import { EntitlementSummary, ModuleAccess, ModuleStanding, ReadOnlyReason } from '@cratis/showrunner.boxoffice';

/**
 * Where a tenant stands with its license, as one word.
 *
 * - `loading`: nothing is known yet.
 * - `suspended`: the operator suspended the tenant; it is read-only.
 * - `trialClosed`: the trial ended without seats being bought; the tenant is read-only.
 * - `trial`: the tenant is on a trial that runs.
 * - `licensed`: the tenant has bought seats.
 * - `unlicensed`: the tenant has no seats and is not on a trial.
 */
export type LicenseState = 'loading' | 'suspended' | 'trialClosed' | 'trial' | 'licensed' | 'unlicensed';

/**
 * A trial that is running or has closed.
 */
export interface TrialStanding {
    /** The last day of the trial. */
    readonly ends: Date;

    /** How many days are left; zero on the last day, and less than zero after it. */
    readonly daysLeft: number;

    /** Whether the trial has closed. */
    readonly isClosed: boolean;
}

/**
 * What a tenant may do, for a component to decide what to show.
 */
export interface Entitlements {
    /** Whether nothing is known about the tenant yet. */
    readonly isLoading: boolean;

    /** Where the tenant stands with its license. */
    readonly state: LicenseState;

    /** Whether the tenant can read but not change anything. */
    readonly isReadOnly: boolean;

    /** Why the tenant is read-only; none when it is not. */
    readonly readOnlyReason: ReadOnlyReason;

    /** How many seats the tenant has today. */
    readonly seats: number;

    /** The trial, when the tenant is on one or was. */
    readonly trial?: TrialStanding;

    /** Whether the product has a customer portal to buy, try and change plan in. */
    readonly canOpenPortal: boolean;

    /** Every module the product declares, and what the tenant may do with it. */
    readonly modules: readonly ModuleStanding[];

    /**
     * Gets whether the tenant has a module: in full or on trial. A preview does not count.
     * @param module The module.
     * @returns True when the tenant has the module.
     */
    has(module: string): boolean;

    /**
     * Gets the access the tenant has to a module.
     * @param module The module.
     * @returns The access; none for a module the product does not declare.
     */
    accessTo(module: string): ModuleAccess;

    /**
     * Gets what the tenant may do with a module.
     * @param module The module.
     * @returns The standing, or undefined for a module the product does not declare.
     */
    standingOf(module: string): ModuleStanding | undefined;
}

const millisecondsInADay = 24 * 60 * 60 * 1000;

/**
 * Turns a day from a proxy into a date.
 * @param day The day: a `DateOnly`, a `yyyy-mm-dd` text, or a date.
 * @returns The date at midnight local time; undefined when there is no day.
 */
export function toDate(day: DateOnly | Date | string | undefined): Date | undefined {
    if (day === undefined || day === null) {
        return undefined;
    }

    if (day instanceof Date) {
        return new Date(day.getFullYear(), day.getMonth(), day.getDate());
    }

    const dateOnly = typeof day === 'string' ? DateOnly.parse(day.substring(0, 10)) : day;
    return dateOnly.toDate();
}

/**
 * Counts the days from one day to another, ignoring the time of day.
 * @param from The day to count from.
 * @param to The day to count to.
 * @returns How many days `to` is after `from`; less than zero when it is before.
 */
export function daysBetween(from: Date, to: Date): number {
    const start = Date.UTC(from.getFullYear(), from.getMonth(), from.getDate());
    const end = Date.UTC(to.getFullYear(), to.getMonth(), to.getDate());
    return Math.round((end - start) / millisecondsInADay);
}

function stateOf(summary: EntitlementSummary, trial: TrialStanding | undefined): LicenseState {
    if (summary.readOnlyReason === ReadOnlyReason.tenantSuspended) {
        return 'suspended';
    }

    if (summary.readOnlyReason === ReadOnlyReason.trialClosed) {
        return 'trialClosed';
    }

    if (summary.isTrial && trial && !trial.isClosed) {
        return 'trial';
    }

    return summary.seats > 0 ? 'licensed' : 'unlicensed';
}

/**
 * Works out what a tenant may do from what the backend says.
 * @param summary The entitlement summary; nothing while it loads.
 * @param today The day it is. Defaults to today.
 * @returns The {@link Entitlements}.
 */
export function entitlementsOf(summary: EntitlementSummary | undefined, today: Date = new Date()): Entitlements {
    if (!summary) {
        return {
            isLoading: true,
            state: 'loading',
            isReadOnly: false,
            readOnlyReason: ReadOnlyReason.none,
            seats: 0,
            canOpenPortal: false,
            modules: [],
            has: () => false,
            accessTo: () => ModuleAccess.none,
            standingOf: () => undefined
        };
    }

    const modules = summary.modules ?? [];
    const ends = summary.isTrial ? toDate(summary.trialEnds) : undefined;
    const daysLeft = ends ? daysBetween(today, ends) : 0;
    const trial = ends
        ? { ends, daysLeft, isClosed: daysLeft < 0 || summary.readOnlyReason === ReadOnlyReason.trialClosed }
        : undefined;
    const standingOf = (module: string) => modules.find(_ => _.module === module);
    const accessTo = (module: string) => standingOf(module)?.access ?? ModuleAccess.none;

    return {
        isLoading: false,
        state: stateOf(summary, trial),
        isReadOnly: summary.readOnlyReason !== ReadOnlyReason.none,
        readOnlyReason: summary.readOnlyReason,
        seats: summary.seats,
        trial,
        canOpenPortal: summary.canOpenPortal,
        modules,
        has: module => accessTo(module) === ModuleAccess.trial || accessTo(module) === ModuleAccess.full,
        accessTo,
        standingOf
    };
}
