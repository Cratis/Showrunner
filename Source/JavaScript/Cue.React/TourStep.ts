// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourCallout } from './TourCallout.js';
import type { TourCamera } from './TourCamera.js';
import type { TourGesture } from './TourGesture.js';

/**
 * One beat of a tour: a line of narration, how much of the tour's stage is showing while it is said, and
 * where the camera and pointer are looking.
 *
 * The player only ever reads these properties. A tour whose steps carry surface-specific extras — such as an
 * editor panel to show — extends this interface and hands the extended step type to the script's
 * {@link TourSection}s; the player carries the extras through untouched.
 */
export interface TourStep {
    /** What the voice-over says, and what the subtitles show. */
    narration: string;

    /**
     * How much of the tour's stage is built by this point. Steps are cumulative — a step never takes
     * anything away, it only adds what the narration has reached.
     */
    stage: number;

    /** Where to move the camera, or omitted to leave it where the previous step left it. */
    camera?: TourCamera;

    /** Where to move the pointer, or omitted to leave it where the previous step left it. */
    gesture?: TourGesture;

    /** A label to pin to the board for the length of the step. */
    callout?: TourCallout;

    /**
     * How long the step lasts. Omitted for narrated steps, which are timed from the narration itself so the
     * script does not have to keep two numbers in sync every time a line is reworded.
     */
    durationMs?: number;
}
