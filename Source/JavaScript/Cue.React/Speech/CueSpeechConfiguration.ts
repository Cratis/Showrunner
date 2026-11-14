// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { CueSpeech } from './CueSpeech.js';

/**
 * How the tour gets its voice-over. This is what an application configures: where the audio for a line comes from.
 */
export interface CueSpeechConfiguration {
    /**
     * The hook that gives the tour its voice. It is called on every render of the tour in the same way, so it must be a
     * stable function, never created during a render.
     */
    useSpeech: () => CueSpeech;

    /**
     * Called from the viewer's first gesture, such as pressing play, to unlock audio playback in browsers that
     * only allow sound after one.
     */
    unlock?: () => void;
}
