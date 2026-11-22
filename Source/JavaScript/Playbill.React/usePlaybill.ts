// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { UsedReleases, useReleases } from './useReleases.js';
import { UsedPlaybillStatus, usePlaybillStatus } from './usePlaybillStatus.js';

/**
 * What {@link usePlaybill} gives a component: the release history and what the person has seen of it.
 */
export type UsedPlaybill = UsedReleases & UsedPlaybillStatus;

/**
 * Reads the release history and observes which releases the signed-in person has not been told about, so a product can
 * draw its own surface instead of {@link Playbill}'s. Must be used inside Arc's context, like every generated query.
 * @returns The releases, and what the person has seen.
 */
export const usePlaybill = (): UsedPlaybill => ({ ...useReleases(), ...usePlaybillStatus() });
