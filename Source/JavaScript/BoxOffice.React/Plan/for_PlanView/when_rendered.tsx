// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { entitlementsFor, moduleStanding, offerOf, summaryOf, trialSummary } from '../../given/boxOfficeData.js';
import { PlanView } from '../PlanView.js';

const offers = [offerOf('Team', ['analytics'], 5, 20)];

const renderPlan = (summary = summaryOf(), withOffers = offers) => render(
    <BoxOfficeHarness entitlements={summary}>
        <PlanView entitlements={entitlementsFor(summary)} offers={withOffers} />
    </BoxOfficeHarness>
);

describe('when rendered', () => {
    afterEach(() => cleanup());

    it('should list every module', () => (renderPlan().queryByText('Agents') !== null).should.be.true);
    it('should say a module is on trial and until when', () => (renderPlan().queryByText('until Nov 11, 2026') !== null).should.be.true);
    it('should offer to try a module the tenant lacks', () => (renderPlan().queryByRole('button', { name: 'Try' }) !== null).should.be.true);
    it('should offer nothing to try when the tenant has every module', () => (renderPlan(summaryOf({ modules: [moduleStanding('core', 'Core', ModuleAccess.full)] })).queryByRole('button', { name: 'Try' }) === null).should.be.true);
    it('should show an offer with its prices', () => (renderPlan().queryByText('$20.00 per month') !== null).should.be.true);
    it('should offer to change plan to a tenant with seats', () => (renderPlan().queryByRole('button', { name: 'Change plan' }) !== null).should.be.true);
    it('should offer to buy to a tenant on a trial', () => (renderPlan(trialSummary()).queryByRole('button', { name: 'Buy' }) !== null).should.be.true);
    it('should say how long the trial has left', () => (renderPlan(trialSummary(12)).queryByText('You are on a trial with 12 days left.') !== null).should.be.true);
    it('should say nothing is on sale', () => (renderPlan(summaryOf(), []).queryByText('Nothing is on sale right now.') !== null).should.be.true);
    it('should say how to change plan when the product has no portal', () => (renderPlan(summaryOf({ canOpenPortal: false })).queryByText('Contact the people who run this product to change your plan.') !== null).should.be.true);
});
