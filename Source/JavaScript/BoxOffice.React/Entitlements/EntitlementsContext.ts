// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, useContext } from 'react';
import { Entitlements, entitlementsOf } from './entitlementModel.js';

/**
 * The React context {@link BoxOfficeProvider} provides the entitlements through. Without a provider nothing is
 * known, so nothing that belongs to a module is shown.
 */
export const EntitlementsContext = createContext<Entitlements>(entitlementsOf(undefined));

/**
 * Gets what the tenant may do.
 * @returns The {@link Entitlements}.
 */
export const useEntitlementsContext = (): Entitlements => useContext(EntitlementsContext);
