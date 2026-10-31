// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { closedTrialSummary, summaryOf, trialSummary } from '../../given/boxOfficeData.js';
import { TrialBanner } from '../TrialBanner.js';

const renderBanner = (entitlements = summaryOf()) => render(<BoxOfficeHarness entitlements={entitlements}><TrialBanner /></BoxOfficeHarness>);

describe('when rendered', () => {
    afterEach(() => cleanup());

    it('should say how long the trial runs', () => (renderBanner(trialSummary(30)).queryByText(/Your trial ends in \d+ days\./) !== null).should.be.true);
    it('should offer to buy during the trial', () => (renderBanner(trialSummary(30)).queryByRole('button', { name: 'Buy' }) !== null).should.be.true);
    it('should say the trial has ended', () => (renderBanner(closedTrialSummary()).queryByText(/Your trial has ended/) !== null).should.be.true);
    it('should show nothing for a tenant that is not on a trial', () => (renderBanner().container.querySelector('.boxoffice-banner') === null).should.be.true);
});
