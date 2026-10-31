// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReadOnlyReason } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, summaryOf, suspendedSummary } from '../../given/boxOfficeData.js';

describe('when the tenant is licensed or suspended', () => {
    const licensed = entitlementsFor(summaryOf());
    const unlicensed = entitlementsFor(summaryOf({ seats: 0 }));
    const suspended = entitlementsFor(suspendedSummary());

    it('should be licensed with seats', () => licensed.state.should.equal('licensed'));
    it('should not be read only when licensed', () => licensed.isReadOnly.should.be.false);
    it('should have no trial when it has none', () => (licensed.trial === undefined).should.be.true);
    it('should be unlicensed without seats', () => unlicensed.state.should.equal('unlicensed'));
    it('should be suspended', () => suspended.state.should.equal('suspended'));
    it('should be read only when suspended', () => suspended.isReadOnly.should.be.true);
    it('should be read only because it is suspended', () => suspended.readOnlyReason.should.equal(ReadOnlyReason.tenantSuspended));
    it('should be suspended even though it has seats', () => suspended.seats.should.equal(10));
});
