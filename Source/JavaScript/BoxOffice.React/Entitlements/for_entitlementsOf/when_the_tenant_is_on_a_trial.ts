// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReadOnlyReason } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, trialSummary } from '../../given/boxOfficeData.js';

describe('when the tenant is on a trial', () => {
    const running = entitlementsFor(trialSummary(12));
    const lastDay = entitlementsFor(trialSummary(0));

    it('should be in the trial state', () => running.state.should.equal('trial'));
    it('should count the days left', () => running.trial!.daysLeft.should.equal(12));
    it('should not have closed the trial', () => running.trial!.isClosed.should.be.false);
    it('should not be read only', () => running.readOnlyReason.should.equal(ReadOnlyReason.none));
    it('should have the seats of the trial', () => running.seats.should.equal(5));
    it('should be on the trial on its last day', () => lastDay.state.should.equal('trial'));
    it('should have no days left on the last day', () => lastDay.trial!.daysLeft.should.equal(0));
});
