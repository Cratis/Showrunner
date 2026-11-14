// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { buildTourTimeline, chapterIndexAtTime, sectionIndexAtTime, stepIndexAtTime } from '../tourTimeline.js';
import { aThreeChapterScript } from './given/a_three_chapter_script.js';

describe('when locating the current step', () => {
    const timeline = buildTourTimeline(aThreeChapterScript);

    it('should report the first step at the very beginning', () => {
        stepIndexAtTime(timeline, 0).should.equal(0);
    });

    it('should report the step a moment is inside', () => {
        stepIndexAtTime(timeline, 1500).should.equal(1);
    });

    it('should hand a boundary to the step that is starting rather than the one that ended', () => {
        stepIndexAtTime(timeline, 3000).should.equal(2);
    });

    it('should keep the last step on screen once the tour has finished', () => {
        stepIndexAtTime(timeline, 10000).should.equal(3);
    });

    it('should report the chapter the moment belongs to', () => {
        chapterIndexAtTime(timeline, 1500).should.equal(0);
        chapterIndexAtTime(timeline, 4000).should.equal(1);
        chapterIndexAtTime(timeline, 8000).should.equal(2);
    });

    it('should report the section the moment belongs to', () => {
        sectionIndexAtTime(timeline, 1500).should.equal(0);
        sectionIndexAtTime(timeline, 4000).should.equal(0);
        sectionIndexAtTime(timeline, 8000).should.equal(1);
    });

    it('should report nothing for an empty script', () => {
        stepIndexAtTime(buildTourTimeline([]), 0).should.equal(-1);
        chapterIndexAtTime(buildTourTimeline([]), 0).should.equal(-1);
        sectionIndexAtTime(buildTourTimeline([]), 0).should.equal(-1);
    });
});
