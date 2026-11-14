// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { formatTourTime } from '../tourTimeline.js';

describe('when formatting a point on the clock', () => {
    it('should pad the seconds to two digits', () => {
        formatTourTime(5000).should.equal('0:05');
    });

    it('should roll seconds over into minutes', () => {
        formatTourTime(95000).should.equal('1:35');
    });

    it('should round part seconds down so the clock never shows a second early', () => {
        formatTourTime(1999).should.equal('0:01');
    });

    it('should show the beginning for anything before it', () => {
        formatTourTime(-500).should.equal('0:00');
    });
});
