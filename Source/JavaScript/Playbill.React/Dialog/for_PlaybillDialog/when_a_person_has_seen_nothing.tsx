// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { act, cleanup, render } from '@testing-library/react';
import { PlaybillProvider } from '../../Configuration/index.js';
import { a_playbill_backend } from '../../given/a_playbill_backend.js';
import { aRelease } from '../../given/releases.js';
import { PlaybillDialog, PlaybillDialogProps } from '../PlaybillDialog.js';

describe('when a person has seen nothing', () => {
    let backend: a_playbill_backend;
    const show = async (props: PlaybillDialogProps = {}) => {
        render(<CratisComponentsProvider><PlaybillProvider client={backend.client}><PlaybillDialog locale='en-US' {...props} /></PlaybillProvider></CratisComponentsProvider>);
        await act(async () => { await new Promise(resolve => setTimeout(resolve, 0)); });
    };

    beforeEach(() => {
        backend = new a_playbill_backend();
        backend.releases = [aRelease({ version: '1.1.0' }), aRelease({ version: '1.0.0' })];
        backend.unreadVersions = ['1.1.0', '1.0.0'];
        backend.lastSeen = '';
    });

    afterEach(() => {
        cleanup();
        backend.restore();
    });

    describe('and the first visit is shown', () => {
        beforeEach(() => show({ firstVisit: 'show' }));

        it('should open on what is unread', () => (document.querySelector('[role="dialog"]') !== null).should.be.true);
        it('should not record anything before it is read', () => backend.marked.should.be.empty);
    });

    describe('and the first visit is not given', () => {
        beforeEach(() => show());

        it('should open on what is unread', () => (document.querySelector('[role="dialog"]') !== null).should.be.true);
    });

    describe('and the first visit is skipped', () => {
        beforeEach(() => show({ firstVisit: 'skip' }));

        it('should not open', () => (document.querySelector('[role="dialog"]') === null).should.be.true);
        it('should not read any release', () => backend.requested.should.be.empty);
        it('should record the newest release as seen', () => backend.marked.should.deep.equal(['1.1.0']));
    });

    describe('and the first visit is skipped for a person who has seen an older release', () => {
        beforeEach(async () => {
            backend.lastSeen = '1.0.0';
            backend.unreadVersions = ['1.1.0'];
            await show({ firstVisit: 'skip' });
        });

        it('should open on what is unread', () => (document.querySelector('[role="dialog"]') !== null).should.be.true);
        it('should not record anything before it is read', () => backend.marked.should.be.empty);
    });
});
