// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCueTours } from './useCueTours.js';

/**
 * What a tour's button gets back from {@link useTourNudge}: whether to point at the tour, and what to do when it starts.
 */
export interface TourNudge {
    /** Whether the start-here pointer should be shown right now. */
    isNudgeShown: boolean;

    /**
     * Records that the person started the tour. The pointer goes away at once on this device and is recorded for every
     * other; call it when the tour starts.
     */
    recordStart(): void;
}

/**
 * Drives one surface's start-here pointer from what the signed-in person has already done: it points at a tour they have
 * never started, and stops once they have, on every device. Must be used inside Arc's context.
 * @param tour The name of the tour the surface offers.
 * @param isSuppressed Whether the surface's own signals say the pointer must not show, such as the person having already
 * done the work the tour teaches. Defaults to false.
 * @returns The {@link TourNudge} for the surface's button.
 * @remarks
 * What the person started has to arrive before the pointer is shown: pointing and then taking the pointer away again is
 * worse than a beat of nothing.
 */
export const useTourNudge = (tour: string, isSuppressed = false): TourNudge => {
    const { isLoaded, hasStarted, recordStart } = useCueTours();
    return {
        isNudgeShown: !isSuppressed && isLoaded && !hasStarted(tour),
        recordStart: () => { void recordStart(tour); }
    };
};
