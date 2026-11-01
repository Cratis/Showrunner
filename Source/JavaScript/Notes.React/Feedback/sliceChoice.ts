// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { formatLocation, LocationParts } from '../Context/formatLocation.js';

/**
 * Gets what identifies a slice location.
 * @param location The location.
 * @returns A key that is the same for the same location.
 */
export function sliceKey(location: LocationParts): string {
    return `${location.module}/${location.feature}/${location.slice}`;
}

/**
 * Removes the slices that repeat an earlier one, and the ones that are empty.
 * @param locations The locations.
 * @returns The distinct, non-empty locations in their order.
 */
export function distinctSlices<TLocation extends LocationParts>(locations: readonly TLocation[]): TLocation[] {
    const seen = new Set<string>();
    return locations.filter(location => {
        const key = sliceKey(location);
        if (key === '//' || seen.has(key)) {
            return false;
        }

        seen.add(key);
        return true;
    });
}

/**
 * Adds a slice to the chosen ones, or removes it when it is chosen already.
 * @param chosen The chosen slices.
 * @param location The slice to toggle.
 * @returns The chosen slices after the toggle.
 */
export function toggleSlice<TLocation extends LocationParts>(chosen: readonly TLocation[], location: TLocation): TLocation[] {
    const key = sliceKey(location);
    return chosen.some(slice => sliceKey(slice) === key)
        ? chosen.filter(slice => sliceKey(slice) !== key)
        : [...chosen, location];
}

/**
 * Whether a slice is among the chosen ones.
 * @param chosen The chosen slices.
 * @param location The slice.
 * @returns True when it is chosen.
 */
export function isChosen(chosen: readonly LocationParts[], location: LocationParts): boolean {
    const key = sliceKey(location);
    return chosen.some(slice => sliceKey(slice) === key);
}

/**
 * Searches the slices the host knows about.
 * @param known Every slice the host knows about.
 * @param query What the user typed; every word has to be part of the module, feature or slice.
 * @returns The distinct slices that match; none for an empty query.
 */
export function searchSlices<TLocation extends LocationParts>(known: readonly TLocation[], query: string): TLocation[] {
    const words = query.toLowerCase().split(/\s+/).filter(word => word.length > 0);
    if (words.length === 0) {
        return [];
    }

    return distinctSlices(known).filter(location => {
        const text = formatLocation(location, ' ', '').toLowerCase();
        return words.every(word => text.includes(word));
    });
}
