// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessDuration } from '@cratis/showrunner.usher';
import { changeAccessChoice, initialAccessChoice } from '../AccessChoice.js';

describe('when choosing how long', () => {
    const choice = initialAccessChoice(false);
    const sevenDays = changeAccessChoice(choice, 'accessDuration', AccessDuration.sevenDays);
    const oneHourFromText = changeAccessChoice(choice, 'accessDuration', '0');
    const unknown = changeAccessChoice(choice, 'accessDuration', 42);
    const otherField = changeAccessChoice(choice, 'summary', 'It broke');

    it('should take the duration', () => sevenDays.accessDuration.should.equal(AccessDuration.sevenDays));
    it('should take a duration given as text', () => oneHourFromText.accessDuration.should.equal(AccessDuration.oneHour));
    it('should ignore a value that is not a duration', () => unknown.accessDuration.should.equal(AccessDuration.oneDay));
    it('should ignore changes of other fields', () => otherField.should.equal(choice));
});
