// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { selfPacedMarkdownFor } from '../tourSelfPaced.js';

describe('when resolving the markdown for a chapter', () => {
    const content = {
        'welcome': 'Welcome to the tour.',
        'read-models': 'Facts are projected into read models.',
    };

    it('should resolve the markdown written for the chapter', () => {
        selfPacedMarkdownFor(content, 'read-models').should.equal('Facts are projected into read models.');
    });

    it('should resolve nothing for a chapter without a walkthrough', () => {
        selfPacedMarkdownFor(content, 'a-chapter-nobody-wrote').should.equal('');
    });

    it('should resolve nothing when the tour brings no content', () => {
        selfPacedMarkdownFor(undefined, 'welcome').should.equal('');
    });

    it('should resolve nothing when there is no chapter to read', () => {
        selfPacedMarkdownFor(content, undefined).should.equal('');
    });
});
