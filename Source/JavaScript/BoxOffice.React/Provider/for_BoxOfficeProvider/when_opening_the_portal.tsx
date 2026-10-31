// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/react';
import { expect } from 'chai';
import sinon from 'sinon';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { PortalButton } from '../../Portal/PortalButton.js';

const renderButton = (portalLink: sinon.SinonStub, openLink: sinon.SinonStub) => render(
    <BoxOfficeHarness portalLink={portalLink} openLink={openLink}>
        <PortalButton intent={PortalIntent.changePlan} label='Change plan' />
    </BoxOfficeHarness>
);

describe('when opening the portal', () => {
    afterEach(() => cleanup());

    describe('and the backend gives no link', () => {
        it('should not open anything', async () => {
            const portalLink = sinon.stub().resolves(undefined);
            const openLink = sinon.stub();
            const result = renderButton(portalLink, openLink);
            fireEvent.click(result.getByRole('button', { name: 'Change plan' }));
            await waitFor(() => portalLink.calledOnce.should.be.true);
            expect(openLink.called).to.equal(false);
        });
    });

    describe('and asking for the link fails', () => {
        it('should not open anything', async () => {
            const portalLink = sinon.stub().rejects(new Error('the portal is not configured'));
            const openLink = sinon.stub();
            const result = renderButton(portalLink, openLink);
            fireEvent.click(result.getByRole('button', { name: 'Change plan' }));
            await waitFor(() => portalLink.calledOnce.should.be.true);
            expect(openLink.called).to.equal(false);
        });
    });
});
