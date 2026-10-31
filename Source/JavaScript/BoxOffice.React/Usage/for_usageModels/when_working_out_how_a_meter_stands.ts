// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { meterStanding } from '../../given/boxOfficeData.js';
import { usageLevel } from '../usageModels.js';

describe('when working out how a meter stands', () => {
    it('should be fine well within the allowance', () => usageLevel(meterStanding({ used: 400 })).should.equal('ok'));
    it('should warn at four fifths of the allowance', () => usageLevel(meterStanding({ used: 800 })).should.equal('warning'));
    it('should be at the allowance when it is used up', () => usageLevel(meterStanding({ used: 1000 })).should.equal('atAllowance'));
    it('should be at the allowance when it is passed', () => usageLevel(meterStanding({ used: 1200 })).should.equal('atAllowance'));
    it('should be at the budget when the budget is reached before the allowance', () => usageLevel(meterStanding({ used: 600, budget: 600 })).should.equal('atBudget'));
    it('should put the allowance before the budget', () => usageLevel(meterStanding({ used: 1000, budget: 600 })).should.equal('atAllowance'));
    it('should not warn without an allowance', () => usageLevel(meterStanding({ used: 50, allowance: 0 })).should.equal('ok'));
});
