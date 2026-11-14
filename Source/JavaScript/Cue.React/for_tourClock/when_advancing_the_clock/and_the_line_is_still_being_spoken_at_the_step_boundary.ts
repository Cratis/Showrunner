// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { advanceTourClock } from '../../tourClock.js';
import { aTwoStepTimeline } from '../given/a_two_step_timeline.js';

describe('when advancing the clock and the line is still being spoken at the step boundary', () => {
    const stillSpeaking = { pendingStepIndex: 0, finished: null, holdStartedAt: null };
    const advance = advanceTourClock(aTwoStepTimeline, 4950, 100, 50000, stillSpeaking);

    it('should hold just short of the boundary', () => {
        advance.timeMs.should.equal(4999);
    });

    it('should keep playing while it holds', () => {
        advance.isPlaying.should.be.true;
    });

    it('should record when the hold began', () => {
        advance.holdStartedAt!.should.equal(50000);
    });

    it('should stay held on the next tick', () => {
        const later = advanceTourClock(aTwoStepTimeline, 4999, 100, 51000, { pendingStepIndex: 0, finished: null, holdStartedAt: 50000 });
        later.timeMs.should.equal(4999);
        later.holdStartedAt!.should.equal(50000);
    });

    it('should not hold a boundary for a line belonging to another step', () => {
        const other = advanceTourClock(aTwoStepTimeline, 4950, 100, 50000, { pendingStepIndex: 1, finished: null, holdStartedAt: null });
        other.timeMs.should.equal(5050);
    });

    it('should hold the end of the tour for the final line', () => {
        const atEnd = advanceTourClock(aTwoStepTimeline, 9950, 100, 50000, { pendingStepIndex: 1, finished: null, holdStartedAt: null });
        atEnd.timeMs.should.equal(9999);
        atEnd.isPlaying.should.be.true;
    });
});
