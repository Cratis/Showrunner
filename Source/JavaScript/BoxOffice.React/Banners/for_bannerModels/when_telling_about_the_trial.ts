// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { closedTrialSummary, entitlementsFor, summaryOf, trialSummary } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { trialBannerModel } from '../bannerModels.js';

describe('when telling about the trial', () => {
    const far = trialBannerModel(entitlementsFor(trialSummary(20)), defaultBoxOfficeLabels)!;
    const near = trialBannerModel(entitlementsFor(trialSummary(7)), defaultBoxOfficeLabels)!;
    const last = trialBannerModel(entitlementsFor(trialSummary(0)), defaultBoxOfficeLabels)!;
    const closed = trialBannerModel(entitlementsFor(closedTrialSummary()), defaultBoxOfficeLabels)!;
    const licensed = trialBannerModel(entitlementsFor(summaryOf()), defaultBoxOfficeLabels);
    const loading = trialBannerModel(entitlementsFor(undefined), defaultBoxOfficeLabels);

    it('should say how many days are left', () => far.text.should.equal('Your trial ends in 20 days.'));
    it('should only inform while the end is far', () => far.severity.should.equal('info'));
    it('should warn when the end is a week away', () => near.severity.should.equal('warn'));
    it('should say the trial ends today on its last day', () => last.text.should.equal('Your trial ends today.'));
    it('should warn on the last day', () => last.severity.should.equal('warn'));
    it('should say the trial has ended', () => closed.text.should.contain('Your trial has ended'));
    it('should be an error when the trial has closed', () => closed.severity.should.equal('error'));
    it('should say nothing to a licensed tenant', () => (licensed === undefined).should.be.true);
    it('should say nothing while loading', () => (loading === undefined).should.be.true);
});
