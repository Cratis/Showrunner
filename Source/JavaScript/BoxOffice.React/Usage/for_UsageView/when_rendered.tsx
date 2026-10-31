// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { meterStanding } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { meterUsageModel } from '../usageModels.js';
import { UsageView } from '../UsageView.js';

const labels = defaultBoxOfficeLabels;
const model = (overrides = {}) => meterUsageModel(meterStanding(overrides), labels, 'en-US');
const renderUsage = (meters = [model()]) => render(<BoxOfficeHarness><UsageView meters={meters} /></BoxOfficeHarness>);

describe('when rendered', () => {
    afterEach(() => cleanup());

    it('should name the meter', () => (renderUsage().queryByText('Agent runs') !== null).should.be.true);
    it('should say what is used against the allowance', () => (renderUsage().queryByText('400 of 1,000 runs') !== null).should.be.true);
    it('should show the use as a progress bar named after the meter', () => (renderUsage().queryByRole('progressbar', { name: 'Use of Agent runs' }) !== null).should.be.true);
    it('should say the allowance is used up', () => (renderUsage([model({ used: 1000 })]).queryByText('Allowance used up') !== null).should.be.true);
    it('should say the budget is reached', () => (renderUsage([model({ used: 600, budget: 600 })]).queryByText('Budget reached') !== null).should.be.true);
    it('should show no progress bar without an allowance', () => (renderUsage([model({ allowance: 0 })]).queryByRole('progressbar') === null).should.be.true);
    it('should show the concurrency column only for a meter that has a limit', () => (renderUsage().queryByText('At the same time') === null).should.be.true);
    it('should show the concurrency column for a meter with a limit', () => (renderUsage([model({ hasConcurrencyLimit: true, concurrencyLimit: 3 })]).queryByText('At the same time') !== null).should.be.true);
    it('should say the product measures nothing', () => (renderUsage([]).queryByText('This product measures nothing.') !== null).should.be.true);
});
