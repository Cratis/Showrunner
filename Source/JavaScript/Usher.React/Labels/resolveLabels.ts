// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultUsherLabels } from './defaultUsherLabels.js';
import { UsherLabelOverrides, UsherLabels } from './UsherLabels.js';

/**
 * Resolves the labels to show: the overrides given, and the English default for every label left out.
 * @param overrides The labels a solution replaces.
 * @returns The {@link UsherLabels}.
 */
export function resolveLabels(overrides?: UsherLabelOverrides): UsherLabels {
    return merge(defaultUsherLabels, overrides) as UsherLabels;
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
