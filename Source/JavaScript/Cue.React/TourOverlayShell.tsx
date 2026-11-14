// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { useEffect, useState } from 'react';
import { useCueConfiguration } from './Configuration/CueProvider.js';
import type { TourSection } from './TourSection.js';
import type { TourStep } from './TourStep.js';
import { TourChapterList } from './TourChapterList.js';
import { TourSelfPacedPanel } from './TourSelfPacedPanel.js';
import { TourSubtitles } from './TourSubtitles.js';
import { TourTimelineBar } from './TourTimelineBar.js';
import { type TourSelfPacedContent, hasSelfPacedContent, selfPacedViewFor } from './tourSelfPaced.js';
import { type TourPlayback, useTourPlayback } from './useTourPlayback.js';

export interface TourOverlayShellProps<TStep extends TourStep = TourStep> {
    /** The scripted sections the tour plays, in order. */
    sections: TourSection<TStep>[];

    /**
     * The written walkthrough of each chapter, keyed by chapter id, for anyone who would rather read the
     * tour at their own pace than listen to it. Omitted, the self-paced switch is not offered.
     */
    selfPacedContent?: TourSelfPacedContent;

    /** Closes the tour and hands the surface back. */
    onClose: () => void;

    /**
     * Renders the tour's stage — the surface the tour drives, such as a board rendered read-only — from the
     * live playback. Called on every playback render; a stage that is expensive to build should read what it
     * needs from the playback inside its own memoized component rather than rebuilding here.
     */
    children: (playback: TourPlayback<TStep>) => React.ReactNode;
}

/**
 * The chrome every guided tour shares: the full-screen layer, the chapter list, the subtitles, the transport
 * with its stop button, and the playback clock driving them all — parameterized by the tour's script and the
 * stage it plays on. There is deliberately no header: a pill in the corner sat on top of whatever the tour
 * was showing there, and everything it offered lives in the transport.
 *
 * The shell owns playback so a tour only brings its script and its stage. Escape stops the tour, exactly
 * like the transport's stop button.
 *
 * A tour that also brings written chapters can be switched into self-paced reading from the transport: the
 * clock and narration stop, the media controls dim, the chapter timings disappear, and the current chapter's
 * walkthrough shows beside the chapter list — with the chapters themselves as the way to move around.
 */
export const TourOverlayShell = <TStep extends TourStep = TourStep>({ sections, selfPacedContent, onClose, children }: TourOverlayShellProps<TStep>) => {
    const playback = useTourPlayback(sections);
    const { stageBoundary: StageBoundary } = useCueConfiguration();
    const [isSelfPaced, setIsSelfPaced] = useState(false);
    const currentChapter = playback.timeline.chapters[playback.chapterIndex]?.chapter;
    const selfPaced = selfPacedViewFor(isSelfPaced, selfPacedContent, currentChapter?.id);

    // Reading is silent: entering self-paced mode stops the clock, and the narration with it. Returning to
    // narrated playback stays paused right where the reader is, until they press play themselves.
    const handleToggleSelfPaced = () => {
        if (!isSelfPaced) playback.pause();
        setIsSelfPaced(!isSelfPaced);
    };

    useEffect(() => {
        const closeOnEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape') onClose();
        };

        window.addEventListener('keydown', closeOnEscape);
        return () => window.removeEventListener('keydown', closeOnEscape);
    }, [onClose]);

    return (
        <div className='cue-overlay'>
            {/* The stage is guarded on its own: a tour that cannot draw its board reports it, and the transport and chapter
                list stay up so the viewer can still stop it, pick another chapter or try again. */}
            <StageBoundary>
                {children(playback)}
            </StageBoundary>

            {selfPaced.isSelfPaced && currentChapter && (
                <TourSelfPacedPanel title={currentChapter.title} markdown={selfPaced.markdown} />
            )}

            <TourChapterList
                sections={playback.timeline.sections}
                chapters={playback.timeline.chapters}
                currentChapterIndex={playback.chapterIndex}
                onSelectChapter={playback.goToChapter}
                showTimings={selfPaced.areTimingsShown}
            />

            {!selfPaced.isSelfPaced && playback.areSubtitlesEnabled && <TourSubtitles narration={playback.step?.narration ?? ''} />}

            <TourTimelineBar
                playback={playback}
                onStop={onClose}
                isSelfPaced={selfPaced.isSelfPaced}
                onToggleSelfPaced={hasSelfPacedContent(selfPacedContent) ? handleToggleSelfPaced : undefined}
            />
        </div>
    );
};
