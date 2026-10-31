// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Importance } from '@cratis/showrunner.marquee';
import { cleanup, render, RenderResult } from '@testing-library/react';
import { ComponentType } from 'react';
import { anAnnouncement } from '../../given/an_announcement.js';
import { defaultMarqueeLabels } from '../../Labels/index.js';
import { MarqueeMarkdownProps } from '../../Markdown/index.js';
import { MarqueeBanner } from '../MarqueeBanner.js';
import { AnnouncementForMe } from '@cratis/showrunner.marquee';

const renderBanner = (announcements: AnnouncementForMe[], markdown?: ComponentType<MarqueeMarkdownProps>): RenderResult =>
    render(<MarqueeBanner announcements={announcements} labels={defaultMarqueeLabels} markdown={markdown} />);

describe('when rendering banners', () => {
    let result: RenderResult;
    let region: HTMLElement;

    afterEach(() => cleanup());

    describe('and there are none', () => {
        beforeEach(() => {
            result = renderBanner([]);
            region = result.getByRole('status');
        });

        it('should still render the live region so the next banner is announced', () => (region.isConnected).should.be.true);
        it('should hold no rows', () => region.children.should.have.length(0));
    });

    describe('and there are three', () => {
        beforeEach(() => {
            result = renderBanner([
                anAnnouncement({ title: 'Down at six', importance: Importance.critical }),
                anAnnouncement({ title: 'Slow search', importance: Importance.warning }),
                anAnnouncement({ title: 'New reports', importance: Importance.information, body: 'Read [more](https://example.com).' })
            ]);
            region = result.getByRole('status');
        });

        it('should announce politely', () => region.getAttribute('aria-live')!.should.equal('polite'));
        it('should be named', () => region.getAttribute('aria-label')!.should.equal('Announcements'));
        it('should show a row for each', () => region.children.should.have.length(3));
        it('should show the title', () => (result.getByText('Down at six').isConnected).should.be.true);
        it('should colour a critical row', () => region.children[0].classList.contains('marquee-banner__item--critical').should.be.true);
        it('should colour a warning row', () => region.children[1].classList.contains('marquee-banner__item--warning').should.be.true);
        it('should colour an information row', () => region.children[2].classList.contains('marquee-banner__item--information').should.be.true);
        it('should name the icon for screen readers', () => region.children[0].querySelector('svg')!.getAttribute('aria-label')!.should.equal('Critical'));
        it('should render the body as markdown', () => region.children[2].querySelector('a')!.getAttribute('href')!.should.equal('https://example.com'));
        it('should have no close button', () => (region.querySelector('button') === null).should.be.true);
    });

    describe('and a markdown renderer is given', () => {
        beforeEach(() => {
            result = renderBanner([anAnnouncement({ body: 'plain' })], ({ source }) => <em data-custom>{source}</em>);
        });

        it('should use it', () => result.container.querySelector('[data-custom]')!.textContent!.should.equal('plain'));
    });
});
