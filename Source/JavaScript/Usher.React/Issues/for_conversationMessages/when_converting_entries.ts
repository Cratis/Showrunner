// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { ConversationAuthorKind, ConversationEntryKind } from '@cratis/showrunner.usher';
import { defaultUsherLabels } from '../../Labels/defaultUsherLabels.js';
import { conversationMessages, isOpenQuestion } from '../conversationMessages.js';
import { an_entry } from '../given/an_issue.js';

describe('when converting entries', () => {
    const issueId = Guid.create();
    const questionId = Guid.create();
    const messages = conversationMessages(issueId, [
        an_entry({ entryId: 1 }),
        an_entry({ entryId: 2, kind: ConversationEntryKind.questionAsked, text: 'Which invoice?', authorKind: ConversationAuthorKind.support, authorName: 'Ada', questionId }),
        an_entry({ entryId: 3, kind: ConversationEntryKind.resolved, text: 'Fixed the rounding', authorKind: ConversationAuthorKind.system }),
        an_entry({ entryId: 4, kind: ConversationEntryKind.fixReleased, text: '4.12.3', authorKind: ConversationAuthorKind.system })
    ], defaultUsherLabels);

    it('should have a message for every entry', () => messages.should.have.lengthOf(4));
    it('should keep the text of what someone wrote', () => messages[1].body.should.equal('Which invoice?'));
    it('should keep the question of a question', () => messages[1].questionId.equals(questionId).should.be.true);
    it('should write a sentence for a resolution', () => messages[2].body.should.equal('Resolved: Fixed the rounding'));
    it('should write a sentence for a release', () => messages[3].body.should.equal('Fixed in 4.12.3'));
    it('should place every message on the issue', () => messages[0].topicId.should.equal(issueId));
    it('should keep the entry ids apart', () => messages.map(message => message.id).should.deep.equal(['1', '2', '3', '4']));
    it('should know an open question', () => isOpenQuestion(questionId).should.be.true);
    it('should know there is no question', () => isOpenQuestion(Guid.empty).should.be.false);
});
