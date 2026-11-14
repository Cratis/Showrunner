// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourAnchorSpace } from './TourAnchorSpace.js';

/**
 * Where the tour's pointer goes for a step, and whether it presses when it gets there. Board (world)
 * coordinates by default, so the pointer stays aimed at the thing it is pointing at while the camera moves;
 * screen coordinates for the tour's own chrome, such as the toolbar.
 */
export interface TourGesture {
    /** The horizontal coordinate to move the pointer to, in the gesture's space. */
    x: number;

    /** The vertical coordinate to move the pointer to, in the gesture's space. */
    y: number;

    /** Whether the pointer presses once it arrives. */
    clicks: boolean;

    /**
     * Where a drag ends, in the gesture's space. When set, the pointer travels to the gesture's point,
     * presses and holds, glides to this point while held, and releases there — how the tour demonstrates
     * dragging something, such as a connection from one module to another.
     */
    dragTo?: { x: number; y: number };

    /** The space the coordinates are expressed in. Board coordinates when omitted. */
    space?: TourAnchorSpace;
}
