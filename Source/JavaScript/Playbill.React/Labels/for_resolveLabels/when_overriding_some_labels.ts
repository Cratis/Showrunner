// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultPlaybillLabels, resolveLabels, withCount } from '../index.js';

describe('when overriding some labels', () => {
    const labels = resolveLabels({ acknowledge: 'Forstått', badgeUnread: '{count} nye' });

    it('should use the texts that were given', () => labels.acknowledge.should.equal('Forstått'));
    it('should keep the English text of the rest', () => labels.close.should.equal(defaultPlaybillLabels.close));
    it('should put the number into a text that has one', () => withCount(labels.badgeUnread, 3).should.equal('3 nye'));
    it('should give the defaults when nothing is overridden', () => resolveLabels().should.deep.equal(defaultPlaybillLabels));
});
