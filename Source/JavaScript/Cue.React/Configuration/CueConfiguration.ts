// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { ComponentType, ReactNode } from 'react';
import type { CueLabels } from '../Labels/CueLabels.js';
import type { CueSpeechConfiguration } from '../Speech/CueSpeechConfiguration.js';

/**
 * What an application tells Cue about itself, once, for every tour it plays.
 */
export interface CueConfiguration {
    /**
     * Where the tour's voice-over comes from. Left out, the tours play silently with their subtitles, and the voice-over
     * button says why there is none. See {@link createHttpSpeech} for a text-to-speech service reached over HTTP.
     */
    speech?: CueSpeechConfiguration;

    /** The texts of the tour's own chrome, such as its buttons. The ones left out stay English. */
    labels?: Partial<CueLabels>;

    /**
     * Guards the stage of a tour: the surface the tour draws. A tour that cannot draw its stage reports it, and the
     * transport and the chapter list stay up so the viewer can still stop the tour or pick another chapter. Left out, a
     * small panel with a Try again button is shown.
     */
    stageBoundary?: ComponentType<{ children: ReactNode }>;
}
