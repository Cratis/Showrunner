// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourStep } from './TourStep.js';

/**
 * A named section of the tour, listed in the sidebar and jumpable from the timeline.
 *
 * Generic over the step type so a tour whose steps carry surface-specific extras keeps its script fully
 * typed; the player itself only reads the {@link TourStep} base.
 */
export interface TourChapter<TStep extends TourStep = TourStep> {
    /** A stable identifier, used as the React key and for jumping to the chapter. */
    id: string;

    /** The title shown in the chapter list. */
    title: string;

    /** One line explaining what the chapter covers, shown under the title. */
    summary: string;

    /** The beats the chapter plays through, in order. */
    steps: TStep[];
}
