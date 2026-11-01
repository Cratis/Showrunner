// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultNotesLabels } from '../defaultNotesLabels.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving with overrides', () => {
    const labels = resolveLabels({
        button: { giveFeedback: 'Gi tilbakemelding' },
        myFeedback: { statuses: { addressed: 'Løst i {version}' } }
    });

    it('should use an overridden label', () => labels.button.giveFeedback.should.equal('Gi tilbakemelding'));
    it('should keep the default of a label left out in the same group', () => labels.button.myFeedback.should.equal(defaultNotesLabels.button.myFeedback));
    it('should keep the defaults of a group left out', () => labels.slices.should.deep.equal(defaultNotesLabels.slices));
    it('should use an overridden label nested deeper', () => labels.myFeedback.statuses.addressed.should.equal('Løst i {version}'));
    it('should keep the default of a label left out nested deeper', () => labels.myFeedback.statuses.given.should.equal(defaultNotesLabels.myFeedback.statuses.given));
    it('should not change the defaults', () => defaultNotesLabels.button.giveFeedback.should.equal('Give feedback'));
});
