// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Where the tour points the camera for a step, in board (world) coordinates so it stays correct whatever the
 * viewport size is.
 */
export interface TourCamera {
    /** The horizontal board coordinate to bring to the center of the viewport. */
    x: number;

    /** The vertical board coordinate to bring to the center of the viewport. */
    y: number;

    /** The zoom level to settle on, where 1 is actual size. */
    zoom: number;
}
