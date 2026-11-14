// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { TourSection } from '../TourSection.js';
import type { TourSelfPacedContent } from '../tourSelfPaced.js';

/**
 * A small script of a made-up order desk, for the stories to show the chrome with: three sections, five chapters.
 */
export const sampleTour: TourSection[] = [
    {
        id: 'welcome',
        title: 'Welcome',
        chapters: [
            {
                id: 'the-desk',
                title: 'The desk',
                summary: 'What the order desk is for.',
                steps: [
                    { narration: 'This is the order desk. Everything a customer asks for lands here first.', stage: 0 },
                    { narration: 'Each order is a card. The newest sits on top.', stage: 1 }
                ]
            },
            {
                id: 'the-cards',
                title: 'The cards',
                summary: 'Reading an order card.',
                steps: [
                    { narration: 'A card says who ordered, what they ordered and where it is going.', stage: 2 }
                ]
            }
        ]
    },
    {
        id: 'working',
        title: 'Working an order',
        chapters: [
            {
                id: 'picking',
                title: 'Picking',
                summary: 'Gathering what was ordered.',
                steps: [
                    { narration: 'Picking an order moves its card to the next lane.', stage: 3 },
                    { narration: 'Anyone can pick, but only one person at a time.', stage: 4 }
                ]
            },
            {
                id: 'shipping',
                title: 'Shipping',
                summary: 'Handing the order to the carrier.',
                steps: [
                    { narration: 'Shipping closes the order and tells the customer it is on its way.', stage: 5 }
                ]
            }
        ]
    },
    {
        id: 'wrapping-up',
        title: 'Wrapping up',
        chapters: [
            {
                id: 'where-next',
                title: 'Where next',
                summary: 'Where to go from here.',
                steps: [
                    { narration: 'That is the desk. Close the tour whenever you like; it will be here when you want it again.', stage: 6 }
                ]
            }
        ]
    }
];

/**
 * The written chapters of the sample tour, as a tour that offers reading it at your own pace would give them.
 */
export const sampleContent: TourSelfPacedContent = {
    'the-desk': 'The order desk is where every order starts.\n\n## What you see\n\n- A card for each order.\n- The newest on top.\n\nOpen a card to read the order.',
    'the-cards': 'A card says **who** ordered, **what** they ordered and **where** it is going.\n\nSee [the shipping guide](https://example.com/shipping) for the carriers we use.',
    'picking': 'Picking moves the card to the next lane.\n\n1. Open the card.\n2. Press **Pick**.\n3. Gather the items.',
    'shipping': 'Shipping closes the order.\n\n```text\npicked -> shipped\n```',
    'where-next': 'That is the desk. Close the tour whenever you like.'
};
