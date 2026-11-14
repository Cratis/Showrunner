// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { buildTourTimeline, narrationsAfter } from '../tourTimeline.js';
import { aThreeChapterScript } from './given/a_three_chapter_script.js';

describe('when looking ahead for narration', () => {
    const timeline = buildTourTimeline(aThreeChapterScript);

    it('should give the lines of the steps that follow, in order', () => {
        narrationsAfter(timeline, 0, 2).should.deep.equal(['Two.', 'Three.']);
    });

    it('should leave out the step being played', () => {
        narrationsAfter(timeline, 1, 1).should.not.include('Two.');
    });

    it('should stop at the end of the tour', () => {
        narrationsAfter(timeline, 2, 5).length.should.equal(1);
    });

    it('should give nothing after the last step', () => {
        narrationsAfter(timeline, timeline.steps.length - 1, 2).should.deep.equal([]);
    });

    it('should give nothing before the tour has a step', () => {
        narrationsAfter(timeline, -1, 2).should.deep.equal([]);
    });
});
