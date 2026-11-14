// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'react';
import { cleanup, render } from '@testing-library/react';
import { CueMarkdown } from '../CueMarkdown.js';

describe('when rendering a chapter', () => {
    afterEach(cleanup);

    describe('and it has a picture', () => {
        it('should show the picture', () => render(<CueMarkdown source='![The desk](media/desk.png)' />).getByAltText('The desk').getAttribute('src')!.should.equal('media/desk.png'));
    });

    describe('and it has a link', () => {
        const link = () => render(<CueMarkdown source='[Guide](https://example.com)' />).getByText('Guide');

        it('should open it in a new tab', () => link().getAttribute('target')!.should.equal('_blank'));
        it('should not hand the page over to it', () => link().getAttribute('rel')!.should.equal('noopener noreferrer'));
    });

    describe('and it has a link that runs script', () => {
        it('should not make it a link', () => (render(<CueMarkdown source='[Click](javascript:alert(1))' />).container.querySelector('a') === null).should.be.true);
    });

    describe('and it has raw html', () => {
        it('should drop it', () => (render(<CueMarkdown source={'Hello <script>alert(1)</script> there'} />).container.querySelector('script') === null).should.be.true);
    });
});
