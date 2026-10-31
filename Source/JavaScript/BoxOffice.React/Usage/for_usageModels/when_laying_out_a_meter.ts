// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { OverageBehavior } from '@cratis/showrunner.boxoffice';
import { meterStanding } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { meterUsageModel } from '../usageModels.js';

describe('when laying out a meter', () => {
    const labels = defaultBoxOfficeLabels;
    const model = meterUsageModel(meterStanding({ used: 400, budget: 600, hasConcurrencyLimit: true, concurrencyLimit: 3 }), labels, 'en-US');
    const parked = meterUsageModel(meterStanding({ overage: OverageBehavior.park }), labels, 'en-US');
    const noAllowance = meterStanding({ allowance: 0, used: 12 });
    const unlimited = meterUsageModel(noAllowance, labels, 'en-US');
    const over = meterUsageModel(meterStanding({ used: 5000 }), labels, 'en-US');
    const unset = meterUsageModel(meterStanding({ hasConcurrencyLimit: true }), labels, 'en-US');

    it('should say what is used against the allowance', () => model.usedText.should.equal('400 of 1,000 runs'));
    it('should tell the share used', () => model.percent.should.equal(40));
    it('should say the period and when it starts over', () => model.periodText.should.equal('per month, starts over Dec 1, 2026'));
    it('should tell the budget', () => model.budgetText.should.equal('600 runs'));
    it('should tell the concurrency limit', () => model.concurrencyText.should.equal('3'));
    it('should say work beyond the allowance is turned away', () => model.overageText.should.equal('Work beyond the allowance is turned away.'));
    it('should say work beyond the allowance waits when it is parked', () => parked.overageText.should.equal('Work beyond the allowance waits for the next period.'));
    it('should say there is no allowance', () => unlimited.usedText.should.equal('12 runs, with no allowance'));
    it('should show no share without an allowance', () => unlimited.percent.should.equal(0));
    it('should not show more than all of the allowance', () => over.percent.should.equal(100));
    it('should say a budget is not set', () => unset.budgetText.should.equal('Not set'));
    it('should say a concurrency limit is not set when the meter has one', () => unset.concurrencyText.should.equal('Not set'));
    it('should show no concurrency limit for a meter without one', () => meterUsageModel(meterStanding(), labels, 'en-US').concurrencyText.should.equal(''));
});
