// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { CueSpeech } from './CueSpeech.js';

/** The voice a tour has when its application gives it none: there is no service, so nothing is said. */
export const silentSpeech: CueSpeech = {
    isAvailable: false,
    isPreparing: false,
    isSpeaking: false,
    hasError: false,
    speak: () => Promise.resolve(),
    prefetch: () => undefined,
    stop: () => undefined
};
