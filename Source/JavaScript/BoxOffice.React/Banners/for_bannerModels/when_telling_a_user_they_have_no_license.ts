// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SeatStatus } from '@cratis/showrunner.boxoffice';
import { closedTrialSummary, entitlementsFor, seatStanding, summaryOf, suspendedSummary } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { noLicenseModel } from '../bannerModels.js';

describe('when telling a user they have no license', () => {
    const labels = defaultBoxOfficeLabels;
    const licensed = entitlementsFor(summaryOf());
    const noSeat = noLicenseModel(licensed, seatStanding(SeatStatus.noSeat), labels)!;
    const overCapacity = noLicenseModel(licensed, seatStanding(SeatStatus.overCapacity), labels)!;
    const fine = noLicenseModel(licensed, seatStanding(SeatStatus.licensed), labels);
    const suspendedWithoutSeat = noLicenseModel(entitlementsFor(suspendedSummary()), seatStanding(SeatStatus.noSeat), labels)!;
    const closed = noLicenseModel(entitlementsFor(closedTrialSummary()), seatStanding(SeatStatus.licensed), labels)!;
    const seatUnknown = noLicenseModel(licensed, undefined, labels);
    const loading = noLicenseModel(entitlementsFor(undefined), seatStanding(SeatStatus.noSeat), labels);

    it('should tell a user without a seat', () => noSeat.reason.should.equal('noSeat'));
    it('should tell them to ask an administrator', () => noSeat.text.should.equal('Ask an administrator of your organization to give you a seat.'));
    it('should tell a user whose seat is not covered', () => overCapacity.reason.should.equal('overCapacity'));
    it('should tell nothing to a user with a seat', () => (fine === undefined).should.be.true);
    it('should put the suspension before the seat', () => suspendedWithoutSeat.reason.should.equal('suspended'));
    it('should tell a user of a tenant whose trial closed', () => closed.reason.should.equal('trialClosed'));
    it('should tell nothing while the seat is not known', () => (seatUnknown === undefined).should.be.true);
    it('should tell nothing while loading', () => (loading === undefined).should.be.true);
});
