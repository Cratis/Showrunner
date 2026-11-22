// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { act, cleanup, fireEvent, render } from '@testing-library/react';
import { PlaybillProvider } from '../../Configuration/index.js';
import { a_playbill_backend } from '../../given/a_playbill_backend.js';
import { aRelease } from '../../given/releases.js';
import { PlaybillHistoryDialog } from '../PlaybillHistoryDialog.js';

describe('when showing the history', () => {
    let backend: a_playbill_backend;
    let closed: number;
    const show = async (open: boolean) => {
        render(
            <CratisComponentsProvider>
                <PlaybillProvider client={backend.client}>
                    <PlaybillHistoryDialog open={open} onClose={() => closed++} locale='en-US' />
                </PlaybillProvider>
            </CratisComponentsProvider>);
        await act(async () => { await new Promise(resolve => setTimeout(resolve, 0)); });
    };

    beforeEach(() => {
        closed = 0;
        backend = new a_playbill_backend();
        backend.releases = [
            aRelease({ version: '1.1.0', sections: [{ name: 'Added', markdown: '- A report page' }] }),
            aRelease({ version: '1.0.0' })
        ];
    });

    afterEach(() => {
        cleanup();
        backend.restore();
    });

    describe('and it is not open', () => {
        beforeEach(() => show(false));

        it('should not be shown', () => (document.querySelector('[role="dialog"]') === null).should.be.true);
        it('should not read any release', () => backend.requested.should.be.empty);
    });

    describe('and it is open', () => {
        beforeEach(() => show(true));

        it('should be shown', () => (document.querySelector('[role="dialog"]') !== null).should.be.true);
        it('should be titled what is new', () => document.querySelector('[role="dialog"]')!.textContent!.should.contain('What\'s new'));
        it('should list every release', () => document.querySelectorAll('.playbill-item').should.have.length(2));
        it('should show the newest release', () => document.querySelector('.playbill-release__version')!.textContent!.should.equal('1.1.0'));

        describe('and it is closed', () => {
            beforeEach(() => fireEvent.click(Array.from(document.querySelectorAll('[role="dialog"] button')).find(button => button.textContent === 'Got it')!));

            it('should say so', () => closed.should.equal(1));
        });
    });
});
