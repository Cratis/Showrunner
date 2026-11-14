// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { advanceTourClock } from '../../tourClock.js';
import { aTwoStepTimeline } from '../given/a_two_step_timeline.js';

describe('when advancing the clock and nothing is being narrated', () => {
    const idle = { pendingStepIndex: null, finished: null, holdStartedAt: null };

    it('should advance by the elapsed time', () => {
        advanceTourClock(aTwoStepTimeline, 1000, 100, 50000, idle).timeMs.should.equal(1100);
    });

    it('should keep playing before the end', () => {
        advanceTourClock(aTwoStepTimeline, 1000, 100, 50000, idle).isPlaying.should.be.true;
    });

    it('should cross a step boundary without pausing', () => {
        advanceTourClock(aTwoStepTimeline, 4950, 100, 50000, idle).timeMs.should.equal(5050);
    });

    it('should stop at the end of the tour', () => {
        const advance = advanceTourClock(aTwoStepTimeline, 9950, 100, 50000, idle);
        advance.timeMs.should.equal(10000);
        advance.isPlaying.should.be.false;
    });
});
