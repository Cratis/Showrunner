// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { StartTour, StartedTour, ByMe } from '@cratis/showrunner.cue';
import { useCallback, useMemo, useState } from 'react';

/**
 * What {@link useCueTours} gives a component.
 */
export interface CueTours {
    /**
     * Whether what the signed-in person has started has arrived. Until it has, {@link hasStarted} knows nothing, so a
     * surface that points at a tour should wait: pointing and then taking the pointer away again is worse than a
     * beat of nothing.
     */
    isLoaded: boolean;

    /**
     * Gets whether the person has started a tour, on this device or any other.
     * @param tour The name of the tour.
     * @returns True when they have; false when they have not, or when it has not arrived yet.
     */
    hasStarted(tour: string): boolean;

    /**
     * Records that the person started a tour. It counts at once on this device and is recorded for every other; starting
     * a tour again changes nothing.
     * @param tour The name of the tour.
     * @returns Resolves when it has been recorded.
     */
    recordStart(tour: string): Promise<void>;
}

/**
 * Knows which guided tours the signed-in person has started, and records the next one. Must be used inside Arc's
 * context, like every generated query.
 * @returns The {@link CueTours}.
 */
export const useCueTours = (): CueTours => {
    const [result] = ByMe.use();
    const [recorded, setRecorded] = useState<ReadonlySet<string>>(() => new Set());

    const started = useMemo(
        () => new Set([...(result.data ?? []).map((tour: StartedTour) => tour.id), ...recorded]),
        [result.data, recorded]);

    const hasStarted = useCallback((tour: string) => started.has(tour), [started]);

    const recordStart = useCallback(async (tour: string) => {
        if (started.has(tour)) {
            return;
        }

        // The answer is held here the moment the tour starts: what the query returns follows the event log and may not
        // have caught up yet.
        setRecorded(current => new Set(current).add(tour));
        const command = new StartTour();
        command.tour = tour;
        await command.execute();
    }, [started]);

    return { isLoaded: result.isReady, hasStarted, recordStart };
};
