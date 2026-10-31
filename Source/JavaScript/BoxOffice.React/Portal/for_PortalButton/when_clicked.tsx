// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/react';
import { expect } from 'chai';
import sinon from 'sinon';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { summaryOf } from '../../given/boxOfficeData.js';
import { PortalButton } from '../PortalButton.js';

describe('when clicked', () => {
    afterEach(() => cleanup());

    describe('and the portal gives a link', () => {
        let portalLink: sinon.SinonStub;
        let openLink: sinon.SinonStub;

        beforeEach(async () => {
            portalLink = sinon.stub().resolves('https://portal.example.com/box-office?signed');
            openLink = sinon.stub();
            const result = render(
                <BoxOfficeHarness portalLink={portalLink} openLink={openLink}>
                    <PortalButton intent={PortalIntent.try} label='Try it' />
                </BoxOfficeHarness>
            );
            fireEvent.click(result.getByRole('button', { name: 'Try it' }));
            await waitFor(() => openLink.calledOnce.should.be.true);
        });

        it('should ask for a link for what the user wants to do', () => portalLink.firstCall.args[0].should.equal(PortalIntent.try));
        it('should ask for no offer', () => (portalLink.firstCall.args[1] === undefined).should.be.true);
        it('should open the link', () => openLink.firstCall.args[0].should.equal('https://portal.example.com/box-office?signed'));
    });

    describe('and the product has no portal', () => {
        it('should not show the button', () => {
            const result = render(
                <BoxOfficeHarness entitlements={summaryOf({ canOpenPortal: false })}>
                    <PortalButton intent={PortalIntent.buy} label='Buy' />
                </BoxOfficeHarness>
            );
            expect(result.queryByRole('button')).to.equal(null);
        });
    });
});
