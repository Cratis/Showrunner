// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '@cratis/fundamentals';
import { daysBetween, toDate } from '../entitlementModel.js';

describe('when turning days into dates', () => {
    const fromDateOnly = toDate(DateOnly.from(2026, 11, 2))!;
    const fromText = toDate('2026-11-02T00:00:00')!;
    const fromDate = toDate(new Date(2026, 10, 2, 17, 30))!;

    it('should turn a date only into midnight local time', () => fromDateOnly.getTime().should.equal(new Date(2026, 10, 2).getTime()));
    it('should turn a text into the same day', () => fromText.getTime().should.equal(new Date(2026, 10, 2).getTime()));
    it('should drop the time of day of a date', () => fromDate.getTime().should.equal(new Date(2026, 10, 2).getTime()));
    it('should turn nothing into nothing', () => (toDate(undefined) === undefined).should.be.true);
    it('should count days ignoring the time of day', () => daysBetween(new Date(2026, 10, 2, 23, 59), new Date(2026, 10, 5, 0, 1)).should.equal(3));
    it('should count days before as negative', () => daysBetween(new Date(2026, 10, 5), new Date(2026, 10, 2)).should.equal(-3));
    it('should count across a daylight saving change as whole days', () => daysBetween(new Date(2026, 9, 24), new Date(2026, 10, 2)).should.equal(9));
});
