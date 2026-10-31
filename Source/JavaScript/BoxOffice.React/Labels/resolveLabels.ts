// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultBoxOfficeLabels } from './defaultBoxOfficeLabels.js';
import { BoxOfficeLabelOverrides, BoxOfficeLabels } from './BoxOfficeLabels.js';

/**
 * Resolves the labels to show: the overrides given, and the English default for every label left out.
 * @param overrides The labels a solution replaces.
 * @returns The {@link BoxOfficeLabels}.
 */
export function resolveLabels(overrides?: BoxOfficeLabelOverrides): BoxOfficeLabels {
    return merge(defaultBoxOfficeLabels, overrides) as BoxOfficeLabels;
}

function merge(defaults: object, overrides?: object): object {
    if (!overrides) {
        return defaults;
    }

    const merged: Record<string, unknown> = { ...defaults };
    for (const [key, value] of Object.entries(overrides)) {
        if (value === undefined) {
            continue;
        }

        const defaultValue = (defaults as Record<string, unknown>)[key];
        merged[key] = isObject(defaultValue) && isObject(value) ? merge(defaultValue, value) : value;
    }

    return merged;
}

function isObject(value: unknown): value is object {
    return typeof value === 'object' && value !== null && !Array.isArray(value);
}
