// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { ErrorDetails } from '@cratis/showrunner.usher';
import { isErrorDetails, toErrorDetails } from '../reportValues.js';

describe('when telling error details from something thrown', () => {
    const details: ErrorDetails = { message: 'Failed', type: 'Error', correlationIds: [], stackFrames: [], location: ArcSliceLocation.empty };
    const location = new ArcSliceLocation('Billing', 'Invoices', 'Pay');

    it('should recognize error details', () => isErrorDetails(details).should.be.true);
    it('should not take an error for error details', () => isErrorDetails(new Error('Failed')).should.be.false);
    it('should use error details as they are', () => toErrorDetails(details).should.equal(details));
    it('should build error details from an error', () => toErrorDetails(new TypeError('Broken'), location).type.should.equal('TypeError'));
    it('should give what was thrown the location', () => toErrorDetails(new TypeError('Broken'), location).location.should.equal(location));
});
