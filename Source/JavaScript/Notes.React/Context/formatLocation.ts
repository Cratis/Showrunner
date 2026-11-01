// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The parts of where in the application something is.
 */
export interface LocationParts {
    /** The module. */
    readonly module: string;
    /** The feature within the module. */
    readonly feature: string;
    /** The slice within the feature. */
    readonly slice: string;
}

/**
 * Formats a location for people, such as `Orders › Editing › Change order`.
 * @param location The location; parts that are empty are left out.
 * @param separator What separates the parts.
 * @param unknown What to show when no part is known.
 * @returns The formatted location.
 */
export function formatLocation(location: LocationParts | undefined, separator: string, unknown: string): string {
    const parts = [location?.module, location?.feature, location?.slice]
        .filter((part): part is string => typeof part === 'string' && part.length > 0);
    return parts.length > 0 ? parts.join(separator) : unknown;
}
