// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Person, SeatHolding, SeatRoster } from '@cratis/showrunner.boxoffice';

/** Whether a seat is covered by what the tenant has bought. */
export type SeatCoverage = 'licensed' | 'overCapacity';

/**
 * One person's seat on the seats page.
 */
export interface SeatRow {
    /** The seat. */
    readonly holding: SeatHolding;

    /** Whether the tenant's seats cover it. */
    readonly coverage: SeatCoverage;
}

/**
 * What the seats page summarizes.
 */
export interface SeatSummary {
    /** How many seats are held. */
    readonly held: number;

    /** How many seats the tenant has. */
    readonly capacity: number;

    /** How many people hold a seat the tenant has no room for. */
    readonly over: number;
}

/**
 * Lays out the seats the way the backend licenses them: the first people to be given a seat, in the order they were
 * given them, are covered by the tenant's seats, and the rest are not.
 * @param roster Who holds the seats.
 * @param capacity How many seats the tenant has.
 * @returns The seats, oldest first.
 */
export function seatRows(roster: SeatRoster | undefined, capacity: number): SeatRow[] {
    const holdings = [...(roster?.holdings ?? [])].sort((a, b) => new Date(a.assignedAt).getTime() - new Date(b.assignedAt).getTime());
    return holdings.map((holding, index) => ({ holding, coverage: index < Math.max(capacity, 0) ? 'licensed' : 'overCapacity' }));
}

/**
 * Summarizes the seats.
 * @param rows The seats from {@link seatRows}.
 * @param capacity How many seats the tenant has.
 * @returns The {@link SeatSummary}.
 */
export function seatSummary(rows: readonly SeatRow[], capacity: number): SeatSummary {
    return { held: rows.length, capacity, over: rows.filter(_ => _.coverage === 'overCapacity').length };
}

/**
 * What the user types to assign a seat.
 */
export interface SeatPersonInput {
    /** The identity provider that knows the person. */
    identityProvider: string;

    /** The person's subject at the identity provider. */
    subject: string;

    /** The person's name. */
    name: string;

    /** The person's email address. */
    email: string;
}

/** An empty {@link SeatPersonInput}. */
export const emptySeatPerson: SeatPersonInput = { identityProvider: '', subject: '', name: '', email: '' };

/**
 * Gets whether what the user typed can be assigned a seat.
 * @param input What the user typed.
 * @returns True when every field has something in it.
 */
export function isValidSeatPerson(input: SeatPersonInput): boolean {
    return Object.values(input).every(value => value.trim().length > 0);
}

/**
 * Makes the person a seat is assigned to.
 * @param input What the user typed.
 * @returns The {@link Person}.
 */
export function personOf(input: SeatPersonInput): Person {
    const person = new Person();
    person.identityProvider = input.identityProvider.trim();
    person.identityProviderSubject = input.subject.trim();
    person.name = input.name.trim();
    person.email = input.email.trim();
    return person;
}
