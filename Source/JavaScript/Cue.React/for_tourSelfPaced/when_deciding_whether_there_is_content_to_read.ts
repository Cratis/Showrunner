// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { hasSelfPacedContent } from '../tourSelfPaced.js';

describe('when deciding whether there is content to read', () => {
    it('should have nothing to read when the tour brings no content at all', () => {
        hasSelfPacedContent(undefined).should.be.false;
    });

    it('should have nothing to read when the content is empty', () => {
        hasSelfPacedContent({}).should.be.false;
    });

    it('should have something to read from the first chapter with markdown', () => {
        hasSelfPacedContent({ welcome: 'Welcome to the tour.' }).should.be.true;
    });
});
