// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { Display, Importance } from '@cratis/showrunner.marquee';
import { cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { vi } from 'vitest';
import { anAnnouncement } from '../../given/an_announcement.js';
import { defaultMarqueeLabels } from '../../Labels/index.js';
import { MarqueeDialog } from '../MarqueeDialog.js';

const renderDialog = (hasMore: boolean, onClose: sinon.SinonStub): RenderResult => render(
    <CratisComponentsProvider>
        <MarqueeDialog
            announcement={anAnnouncement({ title: 'A new module', body: 'Try **reports**.', display: Display.dialogOnce, importance: Importance.warning })}
            hasMore={hasMore}
            onClose={onClose}
            labels={defaultMarqueeLabels} />
    </CratisComponentsProvider>
);

describe('when rendering a dialog', () => {
    let result: RenderResult;
    let onClose: sinon.SinonStub;

    beforeEach(() => {
        onClose = sinon.stub();
    });

    afterEach(() => cleanup());

    describe('and it is the last', () => {
        beforeEach(() => {
            result = renderDialog(false, onClose);
            fireEvent.click(result.getByRole('button', { name: 'Got it' }));
        });

        it('should be a dialog', () => (result.getByRole('dialog').isConnected).should.be.true);
        it('should be named by the title', () => (result.getByText('A new module').isConnected).should.be.true);
        it('should render the body as markdown', () => result.getByText('reports').tagName.should.equal('STRONG'));
        it('should show the importance', () => (result.getByRole('img', { name: 'Warning' }).isConnected).should.be.true);
        it('should close from the button', () => onClose.calledOnce.should.be.true);
    });

    describe('and more follow', () => {
        beforeEach(() => {
            result = renderDialog(true, onClose);
        });

        it('should move on with the next button', () => (result.getByRole('button', { name: 'Next' }).isConnected).should.be.true);
    });

    describe('and focus is placed', () => {
        beforeEach(async () => {
            vi.useFakeTimers();
            result = renderDialog(false, onClose);
            await vi.advanceTimersByTimeAsync(20);
        });

        afterEach(() => vi.useRealTimers());

        it('should not be on the button that closes it', () => (document.activeElement === result.getByRole('button', { name: 'Got it' })).should.be.false);
        it('should be inside the dialog', () => result.getByRole('dialog').contains(document.activeElement).should.be.true);
    });
});
