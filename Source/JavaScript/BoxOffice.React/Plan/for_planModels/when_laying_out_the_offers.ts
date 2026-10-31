// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, offerOf, summaryOf, trialSummary } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { offerRows, planSummary } from '../planModels.js';

describe('when laying out the offers', () => {
    const labels = defaultBoxOfficeLabels;
    const offers = [offerOf('Team', ['analytics'], 5, 20)];
    const trialing = offerRows(offers, entitlementsFor(trialSummary()), labels, 'en-US')[0];
    const licensed = offerRows(offers, entitlementsFor(summaryOf()), labels, 'en-US')[0];

    it('should list the monthly price before the yearly', () => trialing.prices.should.deep.equal(['$20.00 per month', '$200.00 per year']));
    it('should say how many seats are included', () => trialing.seatsText.should.equal('5 seats included'));
    it('should buy when the tenant is on a trial', () => trialing.intent.should.equal(PortalIntent.buy));
    it('should label the button buy on a trial', () => trialing.buttonLabel.should.equal('Buy'));
    it('should change plan when the tenant has seats', () => licensed.intent.should.equal(PortalIntent.changePlan));
    it('should label the button change plan with seats', () => licensed.buttonLabel.should.equal('Change plan'));
    it('should name modules by their names', () => licensed.moduleNames.should.deep.equal(['analytics'].map(_ => entitlementsFor(summaryOf()).standingOf(_)!.name)));
    it('should say how long a trial has left', () => planSummary(entitlementsFor(trialSummary(12)), labels).should.equal('You are on a trial with 12 days left.'));
    it('should say how many seats the plan has', () => planSummary(entitlementsFor(summaryOf()), labels).should.equal('Your plan has 10 seats.'));
    it('should say nothing for a tenant without seats', () => planSummary(entitlementsFor(summaryOf({ seats: 0 })), labels).should.equal(''));
});
