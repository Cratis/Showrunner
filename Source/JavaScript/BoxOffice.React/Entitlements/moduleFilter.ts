// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Entitlements } from './entitlementModel.js';

/**
 * Something that belongs to a module: a navigation entry, a page, a menu item or a route.
 */
export interface ModuleOwned {
    /** The module that owns it; leave it out for what belongs to the product itself and is always shown. */
    module?: string;
}

/**
 * Leaves out what belongs to a module the tenant does not have, so navigation and pages disappear without it. While
 * the entitlements load, what belongs to a module is left out too, rather than shown and taken away.
 * @param items The items, such as the entries of a sidebar.
 * @param entitlements What the tenant may do.
 * @returns The items the tenant may see.
 */
export function filterByModule<TItem extends ModuleOwned>(items: readonly TItem[], entitlements: Entitlements): TItem[] {
    return items.filter(item => item.module === undefined || entitlements.has(item.module));
}
