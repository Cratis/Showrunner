// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SuspensionCause } from '@cratis/showrunner.boxoffice';
import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { suspendedSummary } from '../../given/boxOfficeData.js';
import { SuspensionBannerView } from '../SuspensionBanner.js';

const renderBanner = (cause?: SuspensionCause) => render(
    <BoxOfficeHarness entitlements={suspendedSummary()}>
        <SuspensionBannerView cause={cause} />
    </BoxOfficeHarness>
);

describe('when rendered', () => {
    afterEach(() => cleanup());

    it('should say the organization is suspended', () => (renderBanner().queryByText('Your organization is suspended') !== null).should.be.true);
    it('should say why', () => (renderBanner(SuspensionCause.paymentOverdue).queryByText('A payment is overdue.') !== null).should.be.true);
    it('should say what holds until it is lifted', () => (renderBanner().queryByText(/nothing can be changed until the suspension is lifted/) !== null).should.be.true);
    it('should offer the portal', () => (renderBanner().queryByRole('button', { name: 'Open the customer portal' }) !== null).should.be.true);
});
