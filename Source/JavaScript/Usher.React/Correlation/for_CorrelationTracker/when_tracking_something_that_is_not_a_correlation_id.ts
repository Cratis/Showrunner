// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from '../CorrelationTracker.js';

describe('when tracking something that is not a correlation id', () => {
    const tracker = new CorrelationTracker();
    tracker.track('not a guid');
    tracker.track(Guid.empty);

    it('should keep nothing', () => tracker.correlationIds.should.be.empty);
});
