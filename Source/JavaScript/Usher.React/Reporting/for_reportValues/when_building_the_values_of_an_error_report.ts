// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { AccessDuration, ErrorDetails } from '@cratis/showrunner.usher';
import { reportErrorValues } from '../reportValues.js';

describe('when building the values of an error report', () => {
    const shared = Guid.create();
    const fromError = Guid.create();
    const recent = Guid.create();
    const issueId = Guid.create();
    const error: ErrorDetails = {
        message: 'Invoice 1042 is already paid',
        type: 'InvoiceAlreadyPaid',
        correlationIds: [fromError, shared],
        stackFrames: [],
        location: new ArcSliceLocation('Billing', 'Invoices', 'Pay')
    };
    const values = reportErrorValues(error, {
        issueId,
        location: new ArcSliceLocation('Billing', 'Overview', ''),
        correlationIds: [shared, recent],
        applicationVersion: '4.12.1',
        shareIdentity: true
    });
    const withoutLocation = reportErrorValues({ ...error, location: ArcSliceLocation.empty }, {
        issueId,
        location: new ArcSliceLocation('Billing', 'Overview', ''),
        correlationIds: [],
        applicationVersion: '',
        shareIdentity: false
    });

    it('should carry the issue', () => values.issueId!.should.equal(issueId));
    it('should carry the message', () => values.errorMessage!.should.equal('Invoice 1042 is already paid'));
    it('should carry the type', () => values.errorType!.should.equal('InvoiceAlreadyPaid'));
    it('should carry where the error happened', () => values.location!.feature.should.equal('Invoices'));
    it('should carry the error correlation ids first and each once', () => values.correlationIds!.map(id => id.toString()).should.deep.equal([fromError, shared, recent].map(id => id.toString())));
    it('should carry the version', () => values.applicationVersion!.should.equal('4.12.1'));
    it('should share who the reporter is by default when the provider says so', () => values.shareIdentity!.should.be.true);
    it('should not grant access', () => values.grantAccess!.should.be.false);
    it('should offer access for one day', () => values.accessDuration!.should.equal(AccessDuration.oneDay));
    it('should have no details', () => values.details!.should.equal(''));
    it('should carry where the reporter is when the error does not say', () => withoutLocation.location!.feature.should.equal('Overview'));
});
