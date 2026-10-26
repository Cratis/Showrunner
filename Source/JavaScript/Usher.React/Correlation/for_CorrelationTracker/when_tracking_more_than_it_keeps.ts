// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from '../CorrelationTracker.js';

describe('when tracking more than it keeps', () => {
    const ids = [Guid.create(), Guid.create(), Guid.create(), Guid.create()];
    const tracker = new CorrelationTracker(3);
    ids.forEach(id => tracker.track(id));

    it('should keep as many as it holds', () => tracker.correlationIds.should.have.lengthOf(3));
    it('should drop the oldest', () => tracker.correlationIds.some(id => id.equals(ids[0])).should.be.false);
    it('should keep the most recent last', () => tracker.correlationIds[2].equals(ids[3]).should.be.true);
});
