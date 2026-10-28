// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from '../CorrelationTracker.js';
import { withCorrelationIdHeader } from '../withCorrelationIdHeader.js';

describe('when the request has no correlation id', () => {
    const correlationId = Guid.create();
    const tracker = new CorrelationTracker();
    const headersCallback = withCorrelationIdHeader(() => ({ Authorization: 'Bearer token' }), tracker, 'X-Correlation-ID', () => correlationId);
    const headers = headersCallback() as Record<string, string>;

    it('should add a correlation id', () => headers['x-correlation-id'].should.equal(correlationId.toString()));
    it('should keep the other headers', () => headers.authorization.should.equal('Bearer token'));
    it('should track the correlation id it added', () => tracker.correlationIds[0].equals(correlationId).should.be.true);
});
