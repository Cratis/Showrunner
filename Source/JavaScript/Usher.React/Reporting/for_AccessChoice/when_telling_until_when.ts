// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessDuration } from '@cratis/showrunner.usher';
import { formatClock, formatTime, validUntilFor } from '../AccessChoice.js';

describe('when telling until when', () => {
    const now = new Date(Date.UTC(2026, 9, 27, 17, 10));

    it('should end one hour later', () => validUntilFor(AccessDuration.oneHour, now).toISOString().should.equal('2026-10-27T18:10:00.000Z'));
    it('should end one day later', () => validUntilFor(AccessDuration.oneDay, now).toISOString().should.equal('2026-10-28T17:10:00.000Z'));
    it('should end seven days later', () => validUntilFor(AccessDuration.sevenDays, now).toISOString().should.equal('2026-11-03T17:10:00.000Z'));
    it('should format the day and the time of day', () => formatTime(validUntilFor(AccessDuration.oneDay, now), 'en-GB', 'UTC').should.equal('28 Oct, 17:10'));
    it('should format the time of day on a 24 hour clock', () => formatClock(validUntilFor(AccessDuration.oneHour, now), 'en-US', 'UTC').should.equal('18:10'));
});
