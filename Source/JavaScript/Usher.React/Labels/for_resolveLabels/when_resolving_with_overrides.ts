// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultUsherLabels } from '../defaultUsherLabels.js';
import { resolveLabels } from '../resolveLabels.js';

describe('when resolving with overrides', () => {
    const labels = resolveLabels({
        reportIssue: { title: 'Melde et problem' },
        conversation: { events: { resolved: 'Løst: {text}' } }
    });

    it('should use an overridden label', () => labels.reportIssue.title.should.equal('Melde et problem'));
    it('should keep the default of a label left out in the same group', () => labels.reportIssue.send.should.equal(defaultUsherLabels.reportIssue.send));
    it('should keep the defaults of a group left out', () => labels.myIssues.should.deep.equal(defaultUsherLabels.myIssues));
    it('should use an overridden label nested deeper', () => labels.conversation.events.resolved.should.equal('Løst: {text}'));
    it('should keep the default of a label left out nested deeper', () => labels.conversation.events.accepted.should.equal(defaultUsherLabels.conversation.events.accepted));
    it('should not change the defaults', () => defaultUsherLabels.reportIssue.title.should.equal('Report a problem'));
});
