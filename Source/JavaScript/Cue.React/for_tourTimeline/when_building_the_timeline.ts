// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { buildTourTimeline, narrationDurationMs } from '../tourTimeline.js';
import { aThreeChapterScript } from './given/a_three_chapter_script.js';

describe('when building the timeline', () => {
    const timeline = buildTourTimeline(aThreeChapterScript);

    it('should place every step of every chapter on the clock', () => {
        timeline.steps.should.have.lengthOf(4);
    });

    it('should start the first step at the beginning', () => {
        timeline.steps[0].startMs.should.equal(0);
    });

    it('should run each step straight into the next with no gap', () => {
        timeline.steps[1].startMs.should.equal(timeline.steps[0].endMs);
        timeline.steps[2].startMs.should.equal(timeline.steps[1].endMs);
        timeline.steps[3].startMs.should.equal(timeline.steps[2].endMs);
    });

    it('should last as long as all of its steps together', () => {
        timeline.durationMs.should.equal(10000);
    });

    it('should span each chapter from its first step to its last', () => {
        timeline.chapters[0].startMs.should.equal(0);
        timeline.chapters[0].endMs.should.equal(3000);
        timeline.chapters[1].startMs.should.equal(3000);
        timeline.chapters[1].endMs.should.equal(7000);
    });

    it('should remember which chapter each step belongs to', () => {
        timeline.steps.map(step => step.chapterIndex).should.eql([0, 0, 1, 2]);
    });

    it('should number chapters straight through the sections', () => {
        timeline.chapters.map(chapter => chapter.chapter.id).should.eql(['first', 'second', 'third']);
    });

    it('should remember which section each chapter belongs to', () => {
        timeline.chapters.map(chapter => chapter.sectionIndex).should.eql([0, 0, 1]);
    });

    it('should span each section from its first chapter to its last', () => {
        timeline.sections[0].startMs.should.equal(0);
        timeline.sections[0].endMs.should.equal(7000);
        timeline.sections[1].startMs.should.equal(7000);
        timeline.sections[1].endMs.should.equal(10000);
    });

    it('should time an unmeasured step from how long its narration takes to say', () => {
        const spoken = buildTourTimeline([{
            id: 'only-section',
            title: 'Only section',
            chapters: [{
                id: 'only',
                title: 'Only',
                summary: 'The only chapter.',
                steps: [{ narration: 'A line that has to be read out loud before the tour moves on.', stage: 0 }],
            }],
        }]);

        spoken.durationMs.should.equal(narrationDurationMs('A line that has to be read out loud before the tour moves on.'));
    });
});
