// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultBoxOfficeLabels } from '../defaultBoxOfficeLabels.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving with overrides', () => {
    const labels = resolveLabels({ seats: { title: 'Seter' }, trialBanner: { buy: 'Kjøp' } });

    it('should use an overridden label', () => labels.seats.title.should.equal('Seter'));
    it('should keep the default of a label left out in the same group', () => labels.seats.assign.should.equal(defaultBoxOfficeLabels.seats.assign));
    it('should keep the defaults of a group left out', () => labels.usage.should.deep.equal(defaultBoxOfficeLabels.usage));
    it('should not change the defaults', () => defaultBoxOfficeLabels.seats.title.should.equal('Seats'));
});
