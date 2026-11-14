// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourAnchorSpace } from './TourAnchorSpace.js';

/**
 * A short label the tour pins to a place on the board while a step plays, to name what the narration is
 * talking about.
 */
export interface TourCallout {
    /** The horizontal coordinate to pin the callout to, in the callout's space. */
    x: number;

    /** The vertical coordinate to pin the callout to, in the callout's space. */
    y: number;

    /** The text to show. */
    text: string;

    /** The space the coordinates are expressed in. Board coordinates when omitted. */
    space?: TourAnchorSpace;
}
