// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from '../CorrelationTracker.js';

describe('when tracking the same one again', () => {
    const first = Guid.create();
    const second = Guid.create();
    const tracker = new CorrelationTracker();
    tracker.track(first);
    tracker.track(second);
    tracker.track(first.toString());

    it('should keep it once', () => tracker.correlationIds.should.have.lengthOf(2));
    it('should move it to the end', () => tracker.correlationIds[1].equals(first).should.be.true);
});
