// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChatAuthorKind } from '@cratis/components/Chat';
import { ConversationAuthorKind } from '@cratis/showrunner.usher';
import { defaultUsherLabels } from '../../Labels/defaultUsherLabels.js';
import { authorIdFor, authorOf } from '../conversationMessages.js';

describe('when resolving authors', () => {
    const resolve = (kind: ConversationAuthorKind, name: string) => authorOf(authorIdFor(kind, name), defaultUsherLabels);

    it('should show the reporter as you', () => resolve(ConversationAuthorKind.reporter, '').should.deep.equal({ name: 'You', kind: ChatAuthorKind.User }));
    it('should show a support person by name', () => resolve(ConversationAuthorKind.support, 'Ada').should.deep.equal({ name: 'Ada', kind: ChatAuthorKind.User }));
    it('should show support when the desk hides the name', () => resolve(ConversationAuthorKind.support, '').name.should.equal('Support'));
    it('should show an agent as an agent', () => resolve(ConversationAuthorKind.agent, 'Triage agent').should.deep.equal({ name: 'Triage agent', kind: ChatAuthorKind.Agent }));
    it('should show the system as an update', () => resolve(ConversationAuthorKind.system, '').should.deep.equal({ name: 'Update', kind: ChatAuthorKind.Agent }));
    it('should keep a name with a colon', () => resolve(ConversationAuthorKind.support, 'Ada: Tier 2').name.should.equal('Ada: Tier 2'));
    it('should give two support people different authors', () => authorIdFor(ConversationAuthorKind.support, 'Ada').should.not.equal(authorIdFor(ConversationAuthorKind.support, 'Grace')));
    it('should take the reporter from the labels given', () => authorOf('reporter', { ...defaultUsherLabels, conversation: { ...defaultUsherLabels.conversation, you: 'Deg' } }).name.should.equal('Deg'));
});
