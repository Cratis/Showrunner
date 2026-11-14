// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourTimeline } from '../../tourTimeline.js';

/**
 * A timeline of two steps: the first from 0 to 5000, the second from 5000 to 10000. Handcrafted rather than
 * built from a script, so the boundaries the clock is paced against are plain numbers the specs can read.
 */
export const aTwoStepTimeline: TourTimeline = {
    steps: [
        { step: { narration: 'the first line', stage: 0 }, chapterIndex: 0, startMs: 0, endMs: 5000 },
        { step: { narration: 'the second line', stage: 1 }, chapterIndex: 1, startMs: 5000, endMs: 10000 },
    ],
    chapters: [],
    sections: [],
    durationMs: 10000,
};
