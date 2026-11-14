// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'react';
import { useCueConfiguration } from './Configuration/CueProvider.js';
import { CueIcon } from './Icons.js';
import type { TourPlayback } from './useTourPlayback.js';
import { formatTourTime } from './tourTimeline.js';

export interface TourTimelineBarProps {
    /** The tour's playback state and controls. */
    playback: TourPlayback;

    /** Stops the tour and hands the surface back. Omitted, no stop button is shown. */
    onStop?: () => void;

    /**
     * Whether the tour is being read self-paced. The media playback controls — play, scrubbing, narration
     * and subtitles — dim, while chapter skipping stays live as the way to move through the tour.
     */
    isSelfPaced?: boolean;

    /** Switches between narrated playback and self-paced reading. Omitted, no switch is shown. */
    onToggleSelfPaced?: () => void;
}

/**
 * The tour's transport: play and pause, where it has got to, chapter skipping, and the two switches for how
 * the narration is delivered. When the voice-over has failed, the audio switch carries a quiet warning and
 * pressing it retries — the tour itself never stops for a lost voice. A tour that also has written chapters
 * carries one more switch, into self-paced reading — the transport then stops being a media player: only
 * chapter skipping and the reading switch stay live.
 */
export const TourTimelineBar = ({ playback, onStop, isSelfPaced = false, onToggleSelfPaced }: TourTimelineBarProps) => {
    const { labels: tourStrings, unlockAudio: unlockAudioPlayback } = useCueConfiguration();
    const progress = playback.timeline.durationMs > 0
        ? (playback.timeMs / playback.timeline.durationMs) * 100
        : 0;

    const handleScrub = (event: React.ChangeEvent<HTMLInputElement>) =>
        playback.seekTo(Number(event.target.value));

    // Browsers only allow programmatic audio on media activated by a real user gesture, and narration keeps
    // starting long after any click — so the gestures that lead to narration unlock playback on the way.
    const handlePlayClick = () => {
        unlockAudioPlayback();
        playback.togglePlay();
    };

    const handleAudioClick = () => {
        unlockAudioPlayback();
        playback.toggleAudio();
    };

    const audioTitle = playback.hasNarrationFailed
        ? tourStrings.narrationFailed
        : !playback.isSpeechAvailable
            ? tourStrings.narrationUnavailable
            : playback.isAudioEnabled ? tourStrings.muteNarration : tourStrings.unmuteNarration;

    return (
        <div className={`cue-transport${isSelfPaced ? ' cue-transport--self-paced' : ''}`}>
            <button type='button' className='cue-transport__button' title={tourStrings.previousChapter} onClick={playback.goToPreviousChapter}>
                <CueIcon name='previous' />
            </button>
            <button
                type='button'
                className='cue-transport__button cue-transport__button--primary'
                title={playback.isPlaying ? tourStrings.pause : tourStrings.play}
                disabled={isSelfPaced}
                onClick={handlePlayClick}>
                <CueIcon name={playback.isPlaying ? 'pause' : 'play'} />
            </button>
            <button type='button' className='cue-transport__button' title={tourStrings.nextChapter} onClick={playback.goToNextChapter}>
                <CueIcon name='next' />
            </button>

            <span className='cue-transport__time'>{formatTourTime(playback.timeMs)}</span>
            <div className='cue-transport__track'>
                <div className='cue-transport__elapsed' style={{ width: `${progress}%` }} />
                <input
                    type='range'
                    className='cue-transport__scrubber'
                    aria-label={tourStrings.progress}
                    min={0}
                    max={playback.timeline.durationMs}
                    step={100}
                    value={playback.timeMs}
                    disabled={isSelfPaced}
                    onChange={handleScrub}
                />
            </div>
            <span className='cue-transport__time'>{formatTourTime(playback.timeline.durationMs)}</span>

            <button
                type='button'
                className={`cue-transport__button${playback.isAudioEnabled && playback.isSpeechAvailable ? '' : ' cue-transport__button--off'}${playback.hasNarrationFailed ? ' cue-transport__button--warning' : ''}`}
                title={audioTitle}
                disabled={!playback.isSpeechAvailable || isSelfPaced}
                onClick={handleAudioClick}>
                <CueIcon name={playback.isAudioEnabled && playback.isSpeechAvailable ? 'volumeOn' : 'volumeOff'} />
            </button>
            <button
                type='button'
                className={`cue-transport__button${playback.areSubtitlesEnabled ? '' : ' cue-transport__button--off'}`}
                title={playback.areSubtitlesEnabled ? tourStrings.hideSubtitles : tourStrings.showSubtitles}
                disabled={isSelfPaced}
                onClick={playback.toggleSubtitles}>
                <CueIcon name='subtitles' />
            </button>
            {onToggleSelfPaced && (
                <button
                    type='button'
                    className={`cue-transport__button${isSelfPaced ? ' cue-transport__button--active' : ''}`}
                    title={isSelfPaced ? tourStrings.resumeNarrated : tourStrings.readSelfPaced}
                    onClick={onToggleSelfPaced}>
                    <CueIcon name='book' />
                </button>
            )}
            {onStop && (
                <button type='button' className='cue-transport__button' title={tourStrings.close} onClick={onStop}>
                    <CueIcon name='close' />
                </button>
            )}
        </div>
    );
};
