// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TourOverlayShell, TourSection, useTourNudge } from '@cratis/showrunner.cue.react';
import { useState } from 'react';

const orderDeskTour: TourSection[] = [
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
            }
        ]
    }
];

const writtenChapters = {
    'the-desk': 'The order desk is where every order starts. Open a card to read the order.'
};

export const OrderDeskTour = () => {
    const [isPlaying, setIsPlaying] = useState(false);
    const { isNudgeShown, recordStart } = useTourNudge('order-desk');

    const start = () => {
        recordStart();
        setIsPlaying(true);
    };

    return (
        <>
            <button type='button' onClick={start}>
                {isNudgeShown ? 'Start here: take the tour' : 'Take the tour'}
            </button>
            {isPlaying && (
                <TourOverlayShell sections={orderDeskTour} selfPacedContent={writtenChapters} onClose={() => setIsPlaying(false)}>
                    {playback => <p>The desk has {playback.step?.stage ?? 0} cards on it.</p>}
                </TourOverlayShell>
            )}
        </>
    );
};
