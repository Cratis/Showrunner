// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { errorDetailsFromError } from '../errorDetailsFromError';

class InvoiceAlreadyPaid extends Error {
}

const location = new ArcSliceLocation('Invoicing', 'Invoices', 'PayInvoice');

describe('when building from an error', () => {
    const error = new TypeError('Cannot read properties of undefined');
    const details = errorDetailsFromError(error, location);
    const fromSubclass = errorDetailsFromError(new InvoiceAlreadyPaid('Invoice 1042 is already paid'));

    it('should have the message', () => details.message.should.equal('Cannot read properties of undefined'));
    it('should have the name as the type', () => details.type.should.equal('TypeError'));
    it('should have the stack frames of the error', () => details.stackFrames.should.not.be.empty);
    it('should have no correlation ids', () => details.correlationIds.should.be.empty);
    it('should have the given location', () => details.location.should.equal(location));
    it('should have the class as the type when a subclass keeps the name of error', () => fromSubclass.type.should.equal('InvoiceAlreadyPaid'));
    it('should have an empty location when none is given', () => fromSubclass.location.isEmpty.should.be.true);
});
