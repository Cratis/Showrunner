// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Formats the date a release was made. The date is the one in UTC, so every person reads the same day whatever time
 * zone they are in.
 * @param releasedAt The ISO date the release was made; absent when it was not recorded.
 * @param locale The locale to write it in; the browser's when not given.
 * @returns The date, or an empty string when there is none or it cannot be read.
 */
export const formatReleaseDate = (releasedAt?: string | null, locale?: string | string[]): string => {
    if (!releasedAt) {
        return '';
    }

    const date = new Date(releasedAt);
    if (Number.isNaN(date.getTime())) {
        return '';
    }

    return new Intl.DateTimeFormat(locale, { dateStyle: 'long', timeZone: 'UTC' }).format(date);
};
