// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render, RenderResult } from '@testing-library/react';
import { PlaybillMarkdown } from '../PlaybillMarkdown.js';

describe('when rendering markdown', () => {
    let result: RenderResult;
    const base = '/.cratis/playbill/v1/releases/2.3.0';

    const show = (source: string) => {
        result = render(<PlaybillMarkdown source={source} assetBase={base} />);
    };

    afterEach(() => cleanup());

    describe('and it has a list with emphasis', () => {
        beforeEach(() => show('- A **new** page\n- A fix'));

        it('should render the list items', () => result.container.querySelectorAll('li').should.have.length(2));
        it('should render the emphasis', () => result.container.querySelector('strong')!.textContent!.should.equal('new'));
    });

    describe('and it has a link', () => {
        beforeEach(() => show('[docs](https://example.com/docs)'));

        it('should open in a new tab', () => result.container.querySelector('a')!.getAttribute('target')!.should.equal('_blank'));
        it('should not hand the page to the other', () => result.container.querySelector('a')!.getAttribute('rel')!.should.equal('noopener noreferrer'));
    });

    describe('and it has a link that runs code', () => {
        beforeEach(() => show('[click](javascript:alert(1))'));

        it('should not be a link', () => (result.container.querySelector('a') === null).should.be.true);
        it('should keep the text', () => result.container.textContent!.should.contain('click'));
    });

    describe('and it has raw html', () => {
        beforeEach(() => show('Hello <script>alert(1)</script><b>there</b>'));

        it('should not render a script', () => (result.container.querySelector('script') === null).should.be.true);
        it('should not render other html', () => (result.container.querySelector('b') === null).should.be.true);
    });

    describe('and it has an image captured with the release', () => {
        beforeEach(() => show('![A hero shot](hero.png)'));

        it('should load it from the folder of the release', () => result.container.querySelector('img')!.getAttribute('src')!.should.equal(`${base}/hero.png`));
        it('should keep its description', () => result.container.querySelector('img')!.getAttribute('alt')!.should.equal('A hero shot'));
    });

    describe('and it has an image that is not safe', () => {
        beforeEach(() => show('![x](javascript:alert(1)) ![y](../1.0.0/hero.png)'));

        it('should leave the images out', () => (result.container.querySelector('img') === null).should.be.true);
    });
});
