// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import { anAnnouncement } from '../../given/an_announcement.js';
import { defaultMarqueeLabels } from '../../Labels/index.js';
import { MarqueeBanner } from '../MarqueeBanner.js';

const renderBanner = (title: string, body: string): RenderResult =>
    render(<MarqueeBanner announcements={[anAnnouncement({ title, body })]} labels={defaultMarqueeLabels} />);

describe('when a banner has a message', () => {
    let result: RenderResult;
    let toggle: HTMLElement;
    let message: HTMLElement;
    let tooltip: HTMLElement;

    afterEach(() => cleanup());

    describe('and it is collapsed', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight', 'We upgrade the **database** from 22:00.');
            toggle = result.getByRole('button', { name: 'Show the message' });
            message = result.container.querySelector('.marquee-banner__message')!;
            tooltip = result.container.querySelector('[role=tooltip]')!;
        });

        it('should show the title', () => (result.getByText('Maintenance tonight').isConnected).should.be.true);
        it('should offer to show the message', () => (toggle.isConnected).should.be.true);
        it('should not be expanded', () => toggle.getAttribute('aria-expanded')!.should.equal('false'));
        it('should control the message', () => toggle.getAttribute('aria-controls')!.should.equal(message.id));
        it('should hide the message', () => message.hidden.should.be.true);
        it('should be a button that is operable from the keyboard', () => toggle.tagName.should.equal('BUTTON'));
        it('should be described by the tooltip', () => toggle.getAttribute('aria-describedby')!.should.equal(tooltip.id));
        it('should have a tooltip with the message', () => tooltip.textContent!.should.equal('We upgrade the database from 22:00.'));
        it('should not show the tooltip yet', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.false);
        it('should not show the message as the title', () => result.container.querySelector('.marquee-banner__title')!.textContent!.should.equal('Maintenance tonight'));
    });

    describe('and the button is clicked', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight', 'We upgrade the **database** from 22:00.');
            fireEvent.click(result.getByRole('button', { name: 'Show the message' }));
            toggle = result.getByRole('button', { name: 'Hide the message' });
            message = result.container.querySelector('.marquee-banner__message')!;
        });

        it('should offer to hide the message', () => (toggle.isConnected).should.be.true);
        it('should be expanded', () => toggle.getAttribute('aria-expanded')!.should.equal('true'));
        it('should show the message', () => message.hidden.should.be.false);
        it('should render the message as markdown', () => message.querySelector('strong')!.textContent!.should.equal('database'));

        describe('and it is clicked again', () => {
            beforeEach(() => fireEvent.click(toggle));

            it('should hide the message again', () => message.hidden.should.be.true);
            it('should offer to show the message again', () => (result.getByRole('button', { name: 'Show the message' }).isConnected).should.be.true);
        });
    });

    describe('and the button is hovered', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight', 'See [the status page](https://status.example.com).');
            tooltip = result.container.querySelector('[role=tooltip]')!;
            fireEvent.mouseEnter(result.container.querySelector('.marquee-banner__more')!);
        });

        it('should show the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.true);
        it('should render the message as markdown in it', () => tooltip.querySelector('a')!.getAttribute('href')!.should.equal('https://status.example.com'));

        describe('and the pointer leaves', () => {
            beforeEach(() => fireEvent.mouseLeave(result.container.querySelector('.marquee-banner__more')!));

            it('should hide the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.false);
        });
    });

    describe('and the button is focused', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight', 'We upgrade the database.');
            tooltip = result.container.querySelector('[role=tooltip]')!;
            toggle = result.getByRole('button', { name: 'Show the message' });
            fireEvent.focus(toggle);
        });

        it('should show the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.true);

        describe('and escape is pressed', () => {
            beforeEach(() => fireEvent.keyDown(toggle, { key: 'Escape' }));

            it('should hide the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.false);
        });

        describe('and focus leaves', () => {
            beforeEach(() => fireEvent.blur(toggle));

            it('should hide the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.false);
        });
    });

    describe('and it is expanded while hovered', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight', 'We upgrade the database.');
            tooltip = result.container.querySelector('[role=tooltip]')!;
            fireEvent.mouseEnter(result.container.querySelector('.marquee-banner__more')!);
            fireEvent.click(result.getByRole('button', { name: 'Show the message' }));
        });

        it('should not repeat the message in the tooltip', () => tooltip.classList.contains('marquee-banner__tooltip--visible').should.be.false);
    });

    describe('and the title is markdown with a link', () => {
        beforeEach(() => {
            result = renderBanner('Maintenance tonight. [Read more here.](https://status.example.com)', 'Details.');
        });

        it('should render the link in the title', () => result.container.querySelector('.marquee-banner__title a')!.getAttribute('href')!.should.equal('https://status.example.com'));
        it('should open it in a new tab', () => result.container.querySelector('.marquee-banner__title a')!.getAttribute('rel')!.should.equal('noopener noreferrer'));
    });
});

describe('when a banner has no message', () => {
    let result: RenderResult;

    afterEach(() => cleanup());

    for (const body of ['', '   \n ']) {
        describe(`and the message is ${JSON.stringify(body)}`, () => {
            beforeEach(() => {
                result = renderBanner('Maintenance tonight. [Read more here.](https://status.example.com)', body);
            });

            it('should show the title', () => result.container.querySelector('.marquee-banner__title')!.textContent!.should.equal('Maintenance tonight. Read more here.'));
            it('should have no button', () => (result.container.querySelector('button') === null).should.be.true);
            it('should have no tooltip', () => (result.container.querySelector('[role=tooltip]') === null).should.be.true);
            it('should have no message', () => (result.container.querySelector('.marquee-banner__message') === null).should.be.true);
        });
    }

    describe('and the message is missing', () => {
        beforeEach(() => {
            const announcement = anAnnouncement({ title: 'Maintenance tonight' });
            (announcement as { body?: string }).body = undefined;
            result = render(<MarqueeBanner announcements={[announcement]} labels={defaultMarqueeLabels} />);
        });

        it('should show the title', () => (result.getByText('Maintenance tonight').isConnected).should.be.true);
        it('should have no button', () => (result.container.querySelector('button') === null).should.be.true);
    });
});
