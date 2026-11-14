// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourChapter } from './TourChapter.js';
import type { TourStep } from './TourStep.js';

/**
 * A named part of the tour's curriculum, grouping chapters that belong to one theme — the basics first, then
 * deeper dives. Sections give the chapter list its headings and let a tour grow new material without the
 * basics getting longer.
 *
 * Generic over the step type so a tour whose steps carry surface-specific extras keeps its script fully
 * typed; the player itself only reads the {@link TourStep} base.
 */
export interface TourSection<TStep extends TourStep = TourStep> {
    /** A stable identifier, used as the React key and for locating the section. */
    id: string;

    /** The heading shown above the section's chapters in the chapter list. */
    title: string;

    /** The chapters the section plays through, in order. */
    chapters: TourChapter<TStep>[];
}
