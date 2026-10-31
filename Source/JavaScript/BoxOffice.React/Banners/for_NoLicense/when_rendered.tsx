// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SeatStatus } from '@cratis/showrunner.boxoffice';
import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { closedTrialSummary, seatStanding, summaryOf, suspendedSummary } from '../../given/boxOfficeData.js';
import { NoLicense } from '../NoLicense.js';

describe('when rendered', () => {
    afterEach(() => cleanup());

    describe('for a user without a seat', () => {
        const result = () => render(<BoxOfficeHarness entitlements={summaryOf()} seat={seatStanding(SeatStatus.noSeat)}><NoLicense /></BoxOfficeHarness>);

        it('should say the user has no seat', () => (result().queryByText('You do not have a seat') !== null).should.be.true);
        it('should not offer the portal to someone who cannot buy', () => (result().queryByRole('button') === null).should.be.true);
    });

    describe('for a user of a suspended tenant', () => {
        const result = () => render(<BoxOfficeHarness entitlements={suspendedSummary()} seat={seatStanding(SeatStatus.licensed)}><NoLicense /></BoxOfficeHarness>);

        it('should say the tenant is suspended', () => (result().queryByText('Your organization is suspended') !== null).should.be.true);
        it('should offer the portal', () => (result().queryByRole('button', { name: 'Open the customer portal' }) !== null).should.be.true);
    });

    describe('for a user of a tenant whose trial ended', () => {
        const result = () => render(<BoxOfficeHarness entitlements={closedTrialSummary()} seat={seatStanding(SeatStatus.licensed)}><NoLicense /></BoxOfficeHarness>);

        it('should say the trial ended', () => (result().queryByText('The trial has ended') !== null).should.be.true);
    });

    describe('for a user with a seat', () => {
        const result = () => render(<BoxOfficeHarness entitlements={summaryOf()} seat={seatStanding(SeatStatus.licensed)}><NoLicense /></BoxOfficeHarness>);

        it('should show nothing', () => (result().container.querySelector('.boxoffice-no-license') === null).should.be.true);
    });
});
