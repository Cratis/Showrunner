// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { resolveLabels } from '../resolveLabels.js';

describe('when overriding some labels', () => {
    const labels = resolveLabels({ play: 'Spill av' });

    it('should use the override', () => labels.play.should.equal('Spill av'));
    it('should keep the English default for the rest', () => labels.pause.should.equal('Pause'));
    it('should give the defaults without overrides', () => resolveLabels().play.should.equal('Play'));
});
