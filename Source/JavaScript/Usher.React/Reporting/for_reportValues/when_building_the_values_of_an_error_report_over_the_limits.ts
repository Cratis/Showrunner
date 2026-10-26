// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { ErrorDetails } from '@cratis/showrunner.usher';
import { ReportLimits } from '../ReportLimits.js';
import { reportErrorValues, reportIssueValues } from '../reportValues.js';

describe('when building the values of an error report over the limits', () => {
    const correlationIds = Array.from({ length: ReportLimits.correlationIds + 5 }, () => Guid.create());
    const error: ErrorDetails = {
        message: 'x'.repeat(ReportLimits.errorMessageLength + 1),
        type: 't'.repeat(ReportLimits.errorTypeLength + 1),
        correlationIds,
        stackFrames: Array.from({ length: ReportLimits.stackFrames + 1 }, () => ({ method: 'm'.repeat(ReportLimits.stackFrameTextLength + 1), file: 'f.js', line: 1 })),
        location: new ArcSliceLocation('Billing', 'Invoices', 'Pay')
    };
    const context = {
        issueId: Guid.create(),
        location: new ArcSliceLocation('Billing', 'Overview', ''),
        correlationIds,
        applicationVersion: '4.12.1',
        shareIdentity: false
    };
    const values = reportErrorValues(error, context);
    const issueValues = reportIssueValues(context);

    it('should cut the message', () => values.errorMessage!.length.should.equal(ReportLimits.errorMessageLength));
    it('should cut the type', () => values.errorType!.length.should.equal(ReportLimits.errorTypeLength));
    it('should keep the top stack frames', () => values.stackFrames!.length.should.equal(ReportLimits.stackFrames));
    it('should cut the method of a stack frame', () => values.stackFrames![0].method.length.should.equal(ReportLimits.stackFrameTextLength));
    it('should keep the error correlation ids first', () => values.correlationIds!.map(id => id.toString()).should.deep.equal(correlationIds.slice(0, ReportLimits.correlationIds).map(id => id.toString())));
    it('should keep the most recent correlation ids of an issue report', () => issueValues.correlationIds!.map(id => id.toString()).should.deep.equal(correlationIds.slice(-ReportLimits.correlationIds).map(id => id.toString())));
});
