// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultBoxOfficeLabels } from '../defaultBoxOfficeLabels.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving without overrides', () => {
    it('should be the english defaults', () => resolveLabels().should.deep.equal(defaultBoxOfficeLabels));
});
