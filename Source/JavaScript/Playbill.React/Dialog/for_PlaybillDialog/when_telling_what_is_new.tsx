// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { act, cleanup, fireEvent, render } from '@testing-library/react';
import { PlaybillProvider } from '../../Configuration/index.js';
import { a_playbill_backend } from '../../given/a_playbill_backend.js';
import { aRelease } from '../../given/releases.js';
import { PlaybillDialog, PlaybillDialogProps } from '../PlaybillDialog.js';

describe('when telling what is new', () => {
    let backend: a_playbill_backend;
    const show = async (props: PlaybillDialogProps = {}) => {
        render(<CratisComponentsProvider><PlaybillProvider client={backend.client}><PlaybillDialog locale='en-US' {...props} /></PlaybillProvider></CratisComponentsProvider>);
        await act(async () => { await new Promise(resolve => setTimeout(resolve, 0)); });
    };

    beforeEach(() => {
        backend = new a_playbill_backend();
        backend.releases = [
            aRelease({ version: '1.2.0', sections: [{ name: 'Added', markdown: '- A report page' }] }),
            aRelease({ version: '1.1.0', sections: [{ name: 'Fixed', markdown: '- A crash' }] }),
            aRelease({ version: '1.0.0' })
        ];
    });

    afterEach(() => {
        cleanup();
        backend.restore();
    });

    describe('and nothing is unread', () => {
        beforeEach(() => show());

        it('should not open', () => (document.querySelector('[role="dialog"]') === null).should.be.true);
        it('should not read any release', () => backend.requested.should.be.empty);
    });

    describe('and releases are unread', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.2.0', '1.1.0'];
            await show();
        });

        it('should open by itself', () => (document.querySelector('[role="dialog"]') !== null).should.be.true);
        it('should be titled what is new', () => document.querySelector('[role="dialog"]')!.textContent!.should.contain('What\'s new'));
        it('should show the notes of every unread release', () => document.querySelectorAll('.playbill-release').should.have.length(2));
        it('should show the newest first', () => document.querySelector('.playbill-release__version')!.textContent!.should.equal('1.2.0'));
        it('should not show a release that was seen', () => (document.querySelector('[data-version="1.0.0"]') === null).should.be.true);
        it('should put the releases one level under the dialog heading', () => document.querySelector('.playbill-release__version')!.tagName.should.equal('H3'));
        it('should not record anything before the person has read it', () => backend.marked.should.be.empty);

        describe('and it is closed', () => {
            beforeEach(() => fireEvent.click(document.querySelector('[role="dialog"]')!.querySelector('button:not([aria-label])')!));

            it('should record that the newest release was seen', () => backend.marked.should.deep.equal(['1.2.0']));
            it('should close', () => (document.querySelector('[role="dialog"]') === null).should.be.true);
        });
    });

    describe('and more are unread than the dialog is to show', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.2.0', '1.1.0', '1.0.0'];
            await show({ maxReleases: 1 });
        });

        it('should show only that many', () => document.querySelectorAll('.playbill-release').should.have.length(1));
        it('should point to the rest', () => document.querySelector('.playbill-dialog__earlier')!.textContent!.should.contain('2 earlier releases'));
    });

    describe('and the release view is replaced', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.2.0'];
            await show({ release: ({ release }) => <p data-custom-release>{release.version}</p> });
        });

        it('should draw the release itself', () => document.querySelector('[data-custom-release]')!.textContent!.should.equal('1.2.0'));
    });

    describe('and the endpoints fail', () => {
        beforeEach(async () => {
            backend.unreadVersions = ['1.2.0'];
            backend.failing = true;
            await show();
        });

        it('should say so', () => document.querySelector('[role="alert"]')!.textContent!.should.equal('The release notes could not be loaded.'));
    });
});
