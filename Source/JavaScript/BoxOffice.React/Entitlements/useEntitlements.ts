// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Entitlements } from './entitlementModel.js';
import { useEntitlementsContext } from './EntitlementsContext.js';
import { filterByModule, ModuleOwned } from './moduleFilter.js';

/**
 * Gets what the signed-in user's tenant may do: whether it is on a trial, read-only or suspended, and which modules it
 * has. Use it inside a {@link BoxOfficeProvider}.
 * @returns The {@link Entitlements}.
 */
export function useEntitlements(): Entitlements {
    return useEntitlementsContext();
}

/**
 * Leaves out what belongs to a module the tenant does not have, for the entries of a shell's navigation. Use it
 * inside a {@link BoxOfficeProvider}.
 * @param items The items, each optionally owned by a module.
 * @returns The items the tenant may see.
 */
export function useModuleFilter<TItem extends ModuleOwned>(items: readonly TItem[]): TItem[] {
    return filterByModule(items, useEntitlementsContext());
}
