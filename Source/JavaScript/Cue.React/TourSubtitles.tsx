// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


export interface TourSubtitlesProps {
    /** The line being narrated right now. */
    narration: string;
}

/**
 * Shows what the voice-over is saying, so the tour works with the sound off, on a muted tab, or for anyone
 * who would rather read it.
 */
export const TourSubtitles = ({ narration }: TourSubtitlesProps) => {
    if (narration.length === 0) return null;

    return (
        <div className='cue-subtitles' role='status' aria-live='polite'>
            <p className='cue-subtitles__line'>{narration}</p>
        </div>
    );
};
