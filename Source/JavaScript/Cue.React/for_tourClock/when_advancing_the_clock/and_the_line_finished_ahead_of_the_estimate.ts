// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { advanceTourClock } from '../../tourClock.js';
import { aTwoStepTimeline } from '../given/a_two_step_timeline.js';

describe('when advancing the clock and the line finished ahead of the estimate', () => {
    it('should skip the dead air to the step boundary', () => {
        const breathed = { pendingStepIndex: null, finished: { stepIndex: 0, finishedAt: 49200 }, holdStartedAt: null };
        const advance = advanceTourClock(aTwoStepTimeline, 3000, 100, 50000, breathed);
        advance.timeMs.should.equal(5000);
        advance.finishedConsumed.should.be.true;
    });

    it('should wait out the breath before skipping', () => {
        const justFinished = { pendingStepIndex: null, finished: { stepIndex: 0, finishedAt: 49700 }, holdStartedAt: null };
        advanceTourClock(aTwoStepTimeline, 3000, 100, 50000, justFinished).timeMs.should.equal(3100);
    });

    it('should let a short step play out its choreography', () => {
        const breathed = { pendingStepIndex: null, finished: { stepIndex: 0, finishedAt: 49000 }, holdStartedAt: null };
        advanceTourClock(aTwoStepTimeline, 2000, 100, 50000, breathed).timeMs.should.equal(2100);
    });

    it('should stop rather than skip past the end of the tour', () => {
        const breathed = { pendingStepIndex: null, finished: { stepIndex: 1, finishedAt: 49000 }, holdStartedAt: null };
        const advance = advanceTourClock(aTwoStepTimeline, 8000, 100, 50000, breathed);
        advance.timeMs.should.equal(10000);
        advance.isPlaying.should.be.false;
    });
});
