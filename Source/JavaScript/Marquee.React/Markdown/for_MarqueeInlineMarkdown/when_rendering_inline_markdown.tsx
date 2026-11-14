// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render } from '@testing-library/react';
import { MarqueeInlineMarkdown } from '../MarqueeInlineMarkdown.js';

describe('when rendering inline markdown', () => {
    let container: HTMLElement;
    const renderSource = (source: string) => {
        container = render(<MarqueeInlineMarkdown source={source} />).container;
    };

    afterEach(() => cleanup());

    describe('with a title and a link', () => {
        let link: HTMLAnchorElement;

        beforeEach(() => {
            renderSource('Maintenance tonight. [Read more here.](https://status.example.com)');
            link = container.querySelector('a')!;
        });

        it('should render inside a span', () => container.firstElementChild!.tagName.should.equal('SPAN'));
        it('should keep the text', () => container.textContent!.should.equal('Maintenance tonight. Read more here.'));
        it('should point where it says', () => link.getAttribute('href')!.should.equal('https://status.example.com'));
        it('should open in a new tab', () => link.getAttribute('target')!.should.equal('_blank'));
        it('should not hand the new page the opener', () => link.getAttribute('rel')!.should.equal('noopener noreferrer'));
        it('should not render a paragraph', () => (container.querySelector('p') === null).should.be.true);
    });

    describe('with emphasis and code', () => {
        beforeEach(() => renderSource('A **big** *deal*: run `yarn up`'));

        it('should render strong', () => container.querySelector('strong')!.textContent!.should.equal('big'));
        it('should render emphasis', () => container.querySelector('em')!.textContent!.should.equal('deal'));
        it('should render code', () => container.querySelector('code')!.textContent!.should.equal('yarn up'));
    });

    describe('with block elements', () => {
        beforeEach(() => renderSource('# Heading\n\n- one\n- two\n\n```\ncode block\n```\n\n> quoted'));

        it('should not render a heading', () => (container.querySelector('h1') === null).should.be.true);
        it('should not render a list', () => (container.querySelector('ul, li') === null).should.be.true);
        it('should not render a block quote', () => (container.querySelector('blockquote') === null).should.be.true);
        it('should not render a preformatted block', () => (container.querySelector('pre') === null).should.be.true);
        it('should keep the text', () => container.textContent!.should.contain('Heading').and.contain('one').and.contain('quoted'));
    });

    describe('with embedded html', () => {
        beforeEach(() => renderSource('Hello <script>alert(1)</script><img src=x onerror=alert(1)> world'));

        it('should not render a script', () => (container.querySelector('script') === null).should.be.true);
        it('should not render an image', () => (container.querySelector('img') === null).should.be.true);
    });

    describe('with a markdown image', () => {
        beforeEach(() => renderSource('![tracker](https://example.com/p.png)'));

        it('should leave it out', () => (container.querySelector('img') === null).should.be.true);
    });

    describe('with a javascript link', () => {
        beforeEach(() => renderSource('[click](javascript:alert(1))'));

        it('should not be a link', () => (container.querySelector('a') === null).should.be.true);
        it('should keep the text', () => container.textContent!.should.contain('click'));
    });

    describe('with a mailto link', () => {
        beforeEach(() => renderSource('[write us](mailto:support@example.com)'));

        it('should link it', () => container.querySelector('a')!.getAttribute('href')!.should.equal('mailto:support@example.com'));
    });
});
