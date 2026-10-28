// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessDuration } from '@cratis/showrunner.usher';

/**
 * What the reporter chose about letting support sign in as them.
 */
export interface AccessChoice {
    /** Whether the reporter chose to let support know who they are, apart from granting access. */
    readonly shareIdentity: boolean;

    /** Whether the reporter lets support sign in as them. */
    readonly grantAccess: boolean;

    /** For how long. */
    readonly accessDuration: AccessDuration;
}

/**
 * Gets the choice before the reporter has chosen anything: no access, for one day once they grant it.
 * @param shareIdentity Whether to let support know who the reporter is unless they choose otherwise.
 * @returns The {@link AccessChoice}.
 */
export function initialAccessChoice(shareIdentity: boolean): AccessChoice {
    return { shareIdentity, grantAccess: false, accessDuration: AccessDuration.oneDay };
}

const durationsInMilliseconds: Record<AccessDuration, number> = {
    [AccessDuration.oneHour]: 60 * 60 * 1000,
    [AccessDuration.oneDay]: 24 * 60 * 60 * 1000,
    [AccessDuration.sevenDays]: 7 * 24 * 60 * 60 * 1000
};

/**
 * Applies a change of a command field to the access choice; changes of other fields leave it as it is. While access is
 * granted, sharing who the reporter is is forced, so a change of it is not the reporter's own choice.
 * @param choice The current {@link AccessChoice}.
 * @param fieldName The name of the command field that changed.
 * @param value Its new value.
 * @returns The new {@link AccessChoice}.
 */
export function changeAccessChoice(choice: AccessChoice, fieldName: string, value: unknown): AccessChoice {
    if (fieldName === 'shareIdentity' && !choice.grantAccess) {
        return { ...choice, shareIdentity: value === true };
    }

    if (fieldName === 'grantAccess') {
        return { ...choice, grantAccess: value === true };
    }

    const duration = typeof value === 'string' && value.length > 0 ? Number(value) : value;
    if (fieldName === 'accessDuration' && isAccessDuration(duration)) {
        return { ...choice, accessDuration: duration };
    }

    return choice;
}

/**
 * Gets whether the command shares who the reporter is: always while access is granted, as granting access always
 * shares who the reporter is, and otherwise as the reporter chose.
 * @param choice The {@link AccessChoice}.
 * @returns The value of the command's `shareIdentity`.
 */
export function sharesIdentity(choice: AccessChoice): boolean {
    return choice.grantAccess || choice.shareIdentity;
}

/**
 * Gets when access granted now would end. The backend fixes the end itself when it records the grant; this is what
 * the reporter is told before they send.
 * @param duration The {@link AccessDuration}.
 * @param now What time it is.
 * @returns When access would end.
 */
export function validUntilFor(duration: AccessDuration, now: Date): Date {
    return new Date(now.getTime() + durationsInMilliseconds[duration]);
}

/**
 * Formats a point in time for the access notice and the access panel, such as `27 Oct, 18:10`.
 * @param time The point in time.
 * @param locale The locale to format for; the browser's when not given.
 * @param timeZone The time zone to format in; the browser's when not given.
 * @returns The formatted point in time.
 */
export function formatTime(time: Date, locale?: string, timeZone?: string): string {
    const date = new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short', timeZone }).format(time);
    const clock = new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit', hourCycle: 'h23', timeZone }).format(time);
    return `${date}, ${clock}`;
}

/**
 * Formats the time of day only, such as `18:10`.
 * @param time The point in time.
 * @param locale The locale to format for; the browser's when not given.
 * @param timeZone The time zone to format in; the browser's when not given.
 * @returns The formatted time of day.
 */
export function formatClock(time: Date, locale?: string, timeZone?: string): string {
    return new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit', hourCycle: 'h23', timeZone }).format(time);
}

function isAccessDuration(value: unknown): value is AccessDuration {
    return typeof value === 'number' && Object.hasOwn(durationsInMilliseconds, value);
}
