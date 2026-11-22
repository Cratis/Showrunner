// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { formatReleaseDate } from '../formatReleaseDate.js';

describe('when formatting a date', () => {
    it('should write the day in UTC, so everybody reads the same day', () => formatReleaseDate('2026-03-02T23:30:00-05:00', 'en-US').should.equal('March 3, 2026'));
    it('should write it in the locale', () => formatReleaseDate('2026-03-02T10:00:00Z', 'nb-NO').should.equal('2. mars 2026'));
    it('should give nothing when there is no date', () => formatReleaseDate(undefined, 'en-US').should.equal(''));
    it('should give nothing when the date is null', () => formatReleaseDate(null, 'en-US').should.equal(''));
    it('should give nothing when the date cannot be read', () => formatReleaseDate('soon', 'en-US').should.equal(''));
});
