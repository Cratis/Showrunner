// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReadOnlyReason } from '@cratis/showrunner.boxoffice';
import { closedTrialSummary, entitlementsFor } from '../../given/boxOfficeData.js';

describe('when the trial has closed', () => {
    const entitlements = entitlementsFor(closedTrialSummary());

    it('should be in the trial closed state', () => entitlements.state.should.equal('trialClosed'));
    it('should be read only', () => entitlements.isReadOnly.should.be.true);
    it('should be read only because the trial closed', () => entitlements.readOnlyReason.should.equal(ReadOnlyReason.trialClosed));
    it('should tell the trial is closed', () => entitlements.trial!.isClosed.should.be.true);
    it('should count the days since it ended as negative', () => entitlements.trial!.daysLeft.should.equal(-3));
});
