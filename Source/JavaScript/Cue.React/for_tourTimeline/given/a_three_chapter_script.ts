// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourSection } from '../../TourSection.js';

/**
 * A script of three chapters across two sections, whose every step states its own duration, so the specs can
 * reason about the clock in round numbers instead of against the narration estimate.
 */
export const aThreeChapterScript: TourSection[] = [
    {
        id: 'first-section',
        title: 'First section',
        chapters: [
            {
                id: 'first',
                title: 'First',
                summary: 'The first chapter.',
                steps: [
                    { narration: 'One.', stage: 0, durationMs: 1000 },
                    { narration: 'Two.', stage: 1, durationMs: 2000 },
                ],
            },
            {
                id: 'second',
                title: 'Second',
                summary: 'The second chapter.',
                steps: [
                    { narration: 'Three.', stage: 2, durationMs: 4000 },
                ],
            },
        ],
    },
    {
        id: 'second-section',
        title: 'Second section',
        chapters: [
            {
                id: 'third',
                title: 'Third',
                summary: 'The third chapter.',
                steps: [
                    { narration: 'Four.', stage: 3, durationMs: 3000 },
                ],
            },
        ],
    },
];
