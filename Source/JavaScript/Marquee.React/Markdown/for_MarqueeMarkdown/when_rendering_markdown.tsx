// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render } from '@testing-library/react';
import { MarqueeMarkdown } from '../MarqueeMarkdown.js';

describe('when rendering markdown', () => {
    let container: HTMLElement;
    const renderSource = (source: string, compact = false) => {
        container = render(<MarqueeMarkdown source={source} compact={compact} />).container;
    };

    afterEach(() => cleanup());

    describe('with emphasis', () => {
        beforeEach(() => renderSource('We are **upgrading** tonight'));

        it('should render it as an element', () => container.querySelector('strong')!.textContent!.should.equal('upgrading'));
    });

    describe('with embedded html', () => {
        beforeEach(() => renderSource('Hello <script>alert(1)</script><img src=x onerror=alert(1)> world'));

        it('should not render a script', () => (container.querySelector('script') === null).should.be.true);
        it('should not render the image', () => (container.querySelector('img') === null).should.be.true);
        it('should keep the text', () => container.textContent!.should.contain('Hello'));
    });

    describe('with a markdown image', () => {
        beforeEach(() => renderSource('![tracker](https://example.com/p.png)'));

        it('should leave it out', () => (container.querySelector('img') === null).should.be.true);
    });

    describe('with a safe link', () => {
        let link: HTMLAnchorElement;

        beforeEach(() => {
            renderSource('See [the status page](https://status.example.com)');
            link = container.querySelector('a')!;
        });

        it('should point where it says', () => link.getAttribute('href')!.should.equal('https://status.example.com'));
        it('should open in a new tab', () => link.getAttribute('target')!.should.equal('_blank'));
        it('should not hand the new page the opener', () => link.getAttribute('rel')!.should.equal('noopener noreferrer'));
    });

    describe('with a mailto link', () => {
        beforeEach(() => renderSource('[write us](mailto:support@example.com)'));

        it('should link it', () => container.querySelector('a')!.getAttribute('href')!.should.equal('mailto:support@example.com'));
    });

    describe('with a javascript link', () => {
        beforeEach(() => renderSource('[click](javascript:alert(1))'));

        it('should not be a link', () => (container.querySelector('a') === null).should.be.true);
        it('should keep the text', () => container.textContent!.should.contain('click'));
    });

    describe('with a plain http link', () => {
        beforeEach(() => renderSource('[old](http://example.com)'));

        it('should not be a link', () => (container.querySelector('a') === null).should.be.true);
    });

    describe('compact', () => {
        beforeEach(() => renderSource('one', true));

        it('should be marked for inline rendering', () => container.firstElementChild!.classList.contains('marquee-markdown--compact').should.be.true);
    });
});
