// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultNotesLabels } from '../defaultNotesLabels.js';
import { format } from '../format.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving without overrides', () => {
    it('should give the english defaults', () => resolveLabels().should.equal(defaultNotesLabels));
    it('should fill a placeholder', () => format('{count} chosen', { count: 3 }).should.equal('3 chosen'));
    it('should leave a placeholder without a value', () => format('Remove {slice}', {}).should.equal('Remove {slice}'));
});
