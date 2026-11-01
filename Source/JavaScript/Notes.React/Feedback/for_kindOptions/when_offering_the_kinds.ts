// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { NoteKind } from '@cratis/showrunner.notes';
import { defaultNotesLabels } from '../../Labels/defaultNotesLabels.js';
import { defaultKind, kindOptions } from '../kindOptions.js';

describe('when offering the kinds', () => {
    const options = kindOptions(defaultNotesLabels);

    it('should offer every kind', () => options.map(option => option.value).should.deep.equal([NoteKind.problem, NoteKind.suggestion, NoteKind.question, NoteKind.praise]));
    it('should label them', () => options.map(option => option.label).should.deep.equal(['Problem', 'Suggestion', 'Question', 'Praise']));
    it('should start as a suggestion', () => defaultKind.should.equal(NoteKind.suggestion));
});
