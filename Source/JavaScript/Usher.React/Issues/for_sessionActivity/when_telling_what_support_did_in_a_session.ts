// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConversationEntryKind, SupportSession } from '@cratis/showrunner.usher';
import { defaultUsherLabels } from '../../Labels/index.js';
import { an_entry } from '../given/an_issue.js';
import { sessionActivityOf } from '../sessionActivity.js';

describe('when telling what support did in a session', () => {
    const session = Object.assign(new SupportSession(), { sessionId: 'graces-session', ended: true });
    const entries = [
        an_entry({ entryId: 1, kind: ConversationEntryKind.issueDescribed }),
        an_entry({ entryId: 2, kind: ConversationEntryKind.supportSignedIn, text: '', session: 'graces-session' }),
        an_entry({ entryId: 3, kind: ConversationEntryKind.detailsAdded, text: 'Clicked send twice.', session: 'graces-session' }),
        an_entry({ entryId: 4, kind: ConversationEntryKind.detailsAdded, text: 'In another session.', session: 'agents-session' }),
        an_entry({ entryId: 5, kind: ConversationEntryKind.reopened, text: 'It happened again.', session: 'graces-session' }),
        an_entry({ entryId: 6, kind: ConversationEntryKind.supportSessionEnded, text: '', session: 'graces-session' })
    ];
    const activity = sessionActivityOf(session, entries, defaultUsherLabels.accessPanel.sessionActions);

    it('should tell only what was done in the session, in order', () => activity.map(_ => _.entryId).should.deep.equal(['3', '5']));
    it('should tell what support added', () => activity[0].text.should.equal('Added details: Clicked send twice.'));
    it('should tell that support reopened the issue', () => activity[1].text.should.equal('Reopened the issue: It happened again.'));
});
