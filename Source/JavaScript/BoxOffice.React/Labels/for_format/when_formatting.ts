// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { format } from '../format.js';

describe('when formatting', () => {
    const formatted = format('{held} of {capacity} seats in use', { held: 3, capacity: 10 });
    const missing = format('{days} days left', {});

    it('should fill every placeholder', () => formatted.should.equal('3 of 10 seats in use'));
    it('should leave a placeholder without a value', () => missing.should.equal('{days} days left'));
});
