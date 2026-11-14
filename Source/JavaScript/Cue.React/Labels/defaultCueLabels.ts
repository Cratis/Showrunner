// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { CueLabels } from './CueLabels.js';

/** The English texts, used for every label an application does not give. */
export const defaultCueLabels: CueLabels = {
    close: 'Close the tour',
    play: 'Play',
    pause: 'Pause',
    previousChapter: 'Previous chapter',
    nextChapter: 'Next chapter',
    muteNarration: 'Turn the voice-over off',
    unmuteNarration: 'Turn the voice-over on',
    narrationUnavailable: 'Voice-over is not available',
    narrationFailed: 'Voice-over failed — subtitles carry on. Press to retry',
    hideSubtitles: 'Hide subtitles',
    showSubtitles: 'Show subtitles',
    readSelfPaced: 'Read the tour at your own pace',
    resumeNarrated: 'Return to the narrated tour',
    chapters: 'Chapters',
    progress: 'Tour progress'
};
