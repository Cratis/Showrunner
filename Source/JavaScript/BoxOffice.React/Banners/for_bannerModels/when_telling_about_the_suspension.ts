// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SuspensionCause } from '@cratis/showrunner.boxoffice';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { suspensionReason } from '../bannerModels.js';

describe('when telling about the suspension', () => {
    it('should say the trial ended without seats', () => suspensionReason(SuspensionCause.trialEndedWithoutSeats, defaultBoxOfficeLabels).should.equal('Your trial ended without any seats being bought.'));
    it('should say a payment is overdue', () => suspensionReason(SuspensionCause.paymentOverdue, defaultBoxOfficeLabels).should.equal('A payment is overdue.'));
    it('should say the operator decided', () => suspensionReason(SuspensionCause.administratorDecision, defaultBoxOfficeLabels).should.equal('The people who run this product suspended it.'));
    it('should say nothing when the cause is not known', () => suspensionReason(undefined, defaultBoxOfficeLabels).should.equal(''));
});
