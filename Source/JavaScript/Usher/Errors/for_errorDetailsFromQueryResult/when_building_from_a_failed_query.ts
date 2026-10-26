// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { QueryResult } from '@cratis/arc/queries';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { errorDetailsFromQueryResult } from '../errorDetailsFromQueryResult';

class MyIssues {
    static readonly sliceLocation = new ArcSliceLocation('', '', 'Listing');
}

const correlationId = Guid.parse('9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d');

describe('when building from a failed query', () => {
    const result = new QueryResult({
        data: [],
        isSuccess: false,
        isAuthorized: true,
        isValid: true,
        hasExceptions: true,
        validationResults: [],
        exceptionMessages: ['The read model store is unavailable'],
        exceptionStackTrace: '   at Ledger.Issues.ReportedIssue.MyIssues() in /src/Listing/Listing.cs:line 12',
        paging: { page: 0, size: 0, totalItems: 0, totalPages: 0 }
    }, Object, true);
    const details = errorDetailsFromQueryResult(result, new MyIssues(), undefined, correlationId);
    const withoutCorrelationId = errorDetailsFromQueryResult(result);

    it('should have the exception message', () => details.message.should.equal('The read model store is unavailable'));
    it('should have the stack frames arc returned', () => details.stackFrames.map(_ => _.line).should.deep.equal([12]));
    it('should have the location the query declares', () => details.location.should.equal(MyIssues.sliceLocation));
    it('should have the correlation id given', () => details.correlationIds.should.deep.equal([correlationId]));
    it('should have no correlation ids when none is given', () => withoutCorrelationId.correlationIds.should.be.empty);
});
