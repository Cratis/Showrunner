// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { format } from '../format.js';

describe('when formatting', () => {
    const formatted = format('{name} signed in as you · {start}–{end}', { name: 'Ada', start: '12:03', end: '12:20' });
    const missing = format('Fixed in {version}', {});
    const counted = format('{count} recent requests', { count: 3 });

    it('should fill every placeholder', () => formatted.should.equal('Ada signed in as you · 12:03–12:20'));
    it('should leave a placeholder without a value', () => missing.should.equal('Fixed in {version}'));
    it('should fill a number', () => counted.should.equal('3 recent requests'));
});
