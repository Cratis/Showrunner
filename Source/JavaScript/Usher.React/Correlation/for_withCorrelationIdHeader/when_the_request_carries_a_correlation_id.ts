// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from '../CorrelationTracker.js';
import { withCorrelationIdHeader } from '../withCorrelationIdHeader.js';

describe('when the request carries a correlation id', () => {
    const existing = Guid.create();
    const created = Guid.create();
    const tracker = new CorrelationTracker();
    const headersCallback = withCorrelationIdHeader(() => ({ 'X-Correlation-ID': existing.toString() }), tracker, 'X-Correlation-ID', () => created);
    const headers = headersCallback() as Record<string, string>;

    it('should keep the correlation id it carries', () => headers['x-correlation-id'].should.equal(existing.toString()));
    it('should track the correlation id it carries', () => tracker.correlationIds[0].equals(existing).should.be.true);
    it('should track nothing else', () => tracker.correlationIds.should.have.lengthOf(1));
});
