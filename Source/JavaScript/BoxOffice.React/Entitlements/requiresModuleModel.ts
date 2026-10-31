// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess, ModuleStanding } from '@cratis/showrunner.boxoffice';
import { BoxOfficeLabels, format } from '../Labels/index.js';
import { Entitlements } from './entitlementModel.js';

/**
 * What to tell the user about a module they do not have.
 */
export interface RequiresModuleModel {
    /** Whether the tenant has the module, so the content shows. */
    readonly hasModule: boolean;

    /** The title of the explanation. */
    readonly title: string;

    /** The explanation. */
    readonly text: string;

    /** Whether to offer to try the module. */
    readonly canTry: boolean;
}

/**
 * Works out what to show where a module's content would be.
 * @param entitlements What the tenant may do.
 * @param module The module.
 * @param labels The labels to show.
 * @returns The {@link RequiresModuleModel}.
 */
export function requiresModuleModel(entitlements: Entitlements, module: string, labels: BoxOfficeLabels): RequiresModuleModel {
    const standing: ModuleStanding | undefined = entitlements.standingOf(module);
    const name = standing?.name ?? module;
    const access = standing?.access ?? ModuleAccess.none;
    const text = access === ModuleAccess.preview ? labels.requiresModule.preview : labels.requiresModule.none;

    return {
        hasModule: entitlements.has(module),
        title: format(labels.requiresModule.title, { module: name }),
        text: format(text, { module: name }),
        canTry: access === ModuleAccess.none || access === ModuleAccess.preview
    };
}
