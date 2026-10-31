// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { resolveLabels } from '../resolveLabels.js';

describe('when overriding some labels', () => {
    const labels = resolveLabels({ acknowledge: 'Forstått' });

    it('should use the override', () => labels.acknowledge.should.equal('Forstått'));
    it('should keep the English default for the rest', () => labels.next.should.equal('Next'));
    it('should give the defaults without overrides', () => resolveLabels().acknowledge.should.equal('Got it'));
});
