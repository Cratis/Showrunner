// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { buildTourTimeline, nextChapterTargetMs, previousChapterTargetMs } from '../tourTimeline.js';
import { aThreeChapterScript } from './given/a_three_chapter_script.js';

describe('when stepping between chapters', () => {
    const timeline = buildTourTimeline(aThreeChapterScript);

    it('should go back a chapter when barely into the current one', () => {
        previousChapterTargetMs(timeline, 3500).should.equal(0);
    });

    it('should restart the current chapter when well into it', () => {
        previousChapterTargetMs(timeline, 6000).should.equal(3000);
    });

    it('should stay at the beginning when there is no earlier chapter', () => {
        previousChapterTargetMs(timeline, 500).should.equal(0);
    });

    it('should skip to the start of the following chapter', () => {
        nextChapterTargetMs(timeline, 1500).should.equal(3000);
    });

    it('should run to the end when there is no following chapter', () => {
        nextChapterTargetMs(timeline, 8000).should.equal(10000);
    });
});
