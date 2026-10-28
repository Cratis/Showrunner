// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultUsherLabels } from '../defaultUsherLabels.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving without overrides', () => {
    const labels = resolveLabels();

    it('should be the english defaults', () => labels.should.deep.equal(defaultUsherLabels));
});
