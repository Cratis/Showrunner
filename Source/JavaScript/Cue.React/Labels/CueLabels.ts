// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The texts the tour's own chrome shows. Every one is a prop so an application can localize it. The words of a tour —
 * its narration, its section and chapter titles — are the tour's content and are not here.
 */
export interface CueLabels {
    /** The button that closes the tour. */
    close: string;
    /** The button that starts playback. */
    play: string;
    /** The button that pauses playback. */
    pause: string;
    /** The button that goes back one chapter. */
    previousChapter: string;
    /** The button that skips to the next chapter. */
    nextChapter: string;
    /** The button that switches the voice-over off. */
    muteNarration: string;
    /** The button that switches the voice-over on. */
    unmuteNarration: string;
    /** The explanation shown in place of the voice-over button when no voice-over is available. */
    narrationUnavailable: string;
    /** The explanation shown in place of the voice-over button when the voice-over failed. Pressing the button retries it. */
    narrationFailed: string;
    /** The button that hides the subtitles. */
    hideSubtitles: string;
    /** The button that shows the subtitles. */
    showSubtitles: string;
    /** The button that switches to reading the tour at your own pace. */
    readSelfPaced: string;
    /** The button that goes back to the narrated tour from reading it at your own pace. */
    resumeNarrated: string;
    /** The heading of the chapter list. */
    chapters: string;
    /** The accessible name of the scrubber. */
    progress: string;
}
