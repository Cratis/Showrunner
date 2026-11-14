// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { advanceTourClock } from '../../tourClock.js';
import { aTwoStepTimeline } from '../given/a_two_step_timeline.js';

describe('when advancing the clock and the hold has outlasted its limit', () => {
    const advance = advanceTourClock(aTwoStepTimeline, 4999, 100, 70000, { pendingStepIndex: 0, finished: null, holdStartedAt: 50000 });

    it('should cut the line off and cross the boundary', () => {
        advance.timeMs.should.equal(5099);
    });

    it('should let go of the hold', () => {
        (advance.holdStartedAt === null).should.be.true;
    });
});
