// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { act, cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import { PlaybillProvider } from '../Configuration/index.js';
import { a_playbill_backend } from '../given/a_playbill_backend.js';
import { aRelease } from '../given/releases.js';
import { Playbill, PlaybillProps } from '../Playbill.js';

describe('when showing the releases', () => {
    let backend: a_playbill_backend;
    let result: RenderResult;

    const show = async (props: PlaybillProps = {}) => {
        result = render(<PlaybillProvider client={backend.client}><Playbill locale='en-US' {...props} /></PlaybillProvider>);
        await act(async () => { await new Promise(resolve => setTimeout(resolve, 0)); });
    };

    beforeEach(() => {
        backend = new a_playbill_backend();
        backend.current = '1.1.0';
        backend.releases = [
            aRelease({ version: '1.1.0', summary: 'Faster startup.', sections: [{ name: 'Changed', markdown: '- The dashboard' }] }),
            aRelease({ version: '1.0.0', summary: 'The first release.', releasedAt: '2026-01-05T08:00:00Z' })
        ];
    });

    afterEach(() => {
        cleanup();
        backend.restore();
    });

    describe('and there are releases', () => {
        beforeEach(() => show());

        it('should list every release', () => result.container.querySelectorAll('.playbill-item').should.have.length(2));
        it('should list them newest first', () => result.container.querySelector('.playbill-item__version')!.textContent!.should.equal('1.1.0'));
        it('should say what each is about', () => result.container.querySelector('.playbill-item__summary')!.textContent!.should.equal('Faster startup.'));
        it('should select the newest', () => result.container.querySelector('.playbill-item--selected .playbill-item__version')!.textContent!.should.equal('1.1.0'));
        it('should show the note of the selected release', () => result.container.querySelector('.playbill-release')!.textContent!.should.contain('The dashboard'));
        it('should mark the running version in the list and in the note', () => result.container.querySelectorAll('.playbill-tag--current').should.have.length(2));
        it('should name the list for assistive technology', () => result.getByRole('navigation', { name: 'Releases' }).tagName.should.equal('NAV'));
        it('should say which release is selected', () => result.container.querySelector('[aria-current="true"] .playbill-item__version')!.textContent!.should.equal('1.1.0'));
    });

    describe('and another release is picked', () => {
        let picked: string | undefined;

        beforeEach(async () => {
            await show({ onSelect: version => { picked = version; } });
            fireEvent.click(result.getByRole('button', { name: /1\.0\.0/ }));
            await act(async () => { await new Promise(resolve => setTimeout(resolve, 0)); });
        });

        it('should tell the host', () => picked!.should.equal('1.0.0'));
        it('should show that release', () => result.container.querySelector('.playbill-release__version')!.textContent!.should.equal('1.0.0'));
        it('should select it in the list', () => result.container.querySelector('.playbill-item--selected .playbill-item__version')!.textContent!.should.equal('1.0.0'));
    });

    describe('and the host keeps the selection', () => {
        beforeEach(() => show({ selected: '1.0.0' }));

        it('should show the release the host chose', () => result.container.querySelector('.playbill-release__version')!.textContent!.should.equal('1.0.0'));
    });

    describe('and a release has not been seen', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.1.0'];
            await show();
        });

        it('should tag it in the list', () => result.container.querySelector('.playbill-item .playbill-tag--unread')!.textContent!.should.equal('New'));
        it('should record that it was seen once it is shown', () => backend.marked.should.deep.equal(['1.1.0']));
    });

    describe('and a release has not been seen but the page is told not to record it', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.1.0'];
            await show({ markSeenWhenShown: false });
        });

        it('should not record anything', () => backend.marked.should.be.empty);
    });

    describe('and every release has been seen', () => {
        beforeEach(() => show());

        it('should not record anything', () => backend.marked.should.be.empty);
        it('should not tag anything as new', () => result.container.querySelectorAll('.playbill-tag--unread').should.have.length(0));
    });

    describe('and there are no releases', () => {
        beforeEach(async () => {
            backend.releases = [];
            await show();
        });

        it('should say so', () => result.container.textContent!.should.contain('There are no release notes yet.'));
    });

    describe('and the endpoints fail', () => {
        beforeEach(async () => {
            backend.failing = true;
            await show();
        });

        it('should say so', () => result.getByRole('alert').textContent!.should.equal('The release notes could not be loaded.'));
    });

    describe('and the release view is replaced', () => {
        beforeEach(() => show({ release: ({ release }) => <p data-custom-release>{release.version}</p> }));

        it('should draw the selected release itself', () => result.container.querySelector('[data-custom-release]')!.textContent!.should.equal('1.1.0'));
    });

    describe('and the texts are replaced', () => {
        beforeEach(() => show({ labels: { releases: 'Utgivelser' } }));

        it('should use them', () => result.getByRole('navigation', { name: 'Utgivelser' }).tagName.should.equal('NAV'));
    });
});
