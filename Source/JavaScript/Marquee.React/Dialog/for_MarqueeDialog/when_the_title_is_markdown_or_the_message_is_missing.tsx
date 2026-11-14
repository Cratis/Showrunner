// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { Display } from '@cratis/showrunner.marquee';
import { cleanup, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { anAnnouncement } from '../../given/an_announcement.js';
import { defaultMarqueeLabels } from '../../Labels/index.js';
import { MarqueeDialog } from '../MarqueeDialog.js';

const renderDialog = (title: string, body: string): RenderResult => render(
    <CratisComponentsProvider>
        <MarqueeDialog
            announcement={anAnnouncement({ title, body, display: Display.dialogOnce })}
            hasMore={false}
            onClose={sinon.stub()}
            labels={defaultMarqueeLabels} />
    </CratisComponentsProvider>
);

describe('when the title of a dialog is markdown or the message is missing', () => {
    let result: RenderResult;

    afterEach(() => cleanup());

    describe('and the title has a link', () => {
        beforeEach(() => {
            result = renderDialog('Maintenance tonight. [Read more here.](https://status.example.com)', 'We upgrade **everything**.');
        });

        it('should be named by the title as plain text', () => result.getByRole('heading', { name: 'Maintenance tonight. Read more here.' }).isConnected.should.be.true);
        it('should not put markdown in the heading', () => result.getByRole('heading').textContent!.should.not.contain('https://'));
        it('should show the title with its link', () => result.container.ownerDocument.querySelector('.marquee-dialog__title a')!.getAttribute('href')!.should.equal('https://status.example.com'));
        it('should show the message', () => result.getByText('everything').tagName.should.equal('STRONG'));
    });

    describe('and the title is plain', () => {
        beforeEach(() => {
            result = renderDialog('Meet reports', 'Try them.');
        });

        it('should not show the title twice', () => (result.container.ownerDocument.querySelector('.marquee-dialog__title') === null).should.be.true);
    });

    describe('and there is no message', () => {
        beforeEach(() => {
            result = renderDialog('Maintenance tonight', '');
        });

        it('should show the title', () => (result.getByRole('heading', { name: 'Maintenance tonight' }).isConnected).should.be.true);
        it('should show no message', () => (result.container.ownerDocument.querySelector('.marquee-markdown') === null).should.be.true);
        it('should still be closable', () => (result.getByRole('button', { name: 'Got it' }).isConnected).should.be.true);
    });
});
