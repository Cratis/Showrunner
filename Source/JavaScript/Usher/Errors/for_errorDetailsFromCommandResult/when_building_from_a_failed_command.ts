// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandResult } from '@cratis/arc/commands';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { errorDetailsFromCommandResult } from '../errorDetailsFromCommandResult';

class PayInvoice {
    static readonly sliceLocation = new ArcSliceLocation('Invoicing', 'Invoices', 'PayInvoice');
}

const correlationId = Guid.parse('3f2504e0-4f89-11d3-9a0c-0305e82c3301');

describe('when building from a failed command', () => {
    const result = new CommandResult({
        correlationId: correlationId.toString(),
        isSuccess: false,
        isAuthorized: true,
        isValid: true,
        hasExceptions: true,
        validationResults: [],
        exceptionMessages: ['Invoice 1042 is already paid'],
        exceptionStackTrace: '   at Ledger.Invoicing.PayInvoice.Handle() in /src/Invoicing/PayInvoice.cs:line 57',
        authorizationFailureReason: '',
        response: undefined
    }, Object, false);
    const details = errorDetailsFromCommandResult(result, new PayInvoice(), new ArcSliceLocation('Invoicing', '', ''));

    it('should have the exception message', () => details.message.should.equal('Invoice 1042 is already paid'));
    it('should have no type, which the backend knows', () => details.type.should.equal(''));
    it('should have the correlation id of the command', () => details.correlationIds.map(_ => _.toString()).should.deep.equal([correlationId.toString()]));
    it('should have the stack frames arc returned', () => details.stackFrames.map(_ => _.method).should.deep.equal(['Ledger.Invoicing.PayInvoice.Handle()']));
    it('should have the location the command declares', () => details.location.should.equal(PayInvoice.sliceLocation));
});
