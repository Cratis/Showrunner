// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { advanceTourClock } from '../../tourClock.js';
import { aTwoStepTimeline } from '../given/a_two_step_timeline.js';

describe('when advancing the clock and the held line just finished', () => {
    it('should keep holding through the breath', () => {
        const settling = { pendingStepIndex: null, finished: { stepIndex: 0, finishedAt: 49800 }, holdStartedAt: 48000 };
        advanceTourClock(aTwoStepTimeline, 4999, 100, 50000, settling).timeMs.should.equal(4999);
    });

    it('should release across the boundary once the breath has passed', () => {
        const breathed = { pendingStepIndex: null, finished: { stepIndex: 0, finishedAt: 49000 }, holdStartedAt: 48000 };
        const advance = advanceTourClock(aTwoStepTimeline, 4999, 100, 50000, breathed);
        advance.timeMs.should.equal(5099);
        (advance.holdStartedAt === null).should.be.true;
        advance.finishedConsumed.should.be.true;
    });
});
