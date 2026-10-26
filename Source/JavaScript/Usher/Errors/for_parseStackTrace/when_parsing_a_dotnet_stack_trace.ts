// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { parseStackTrace } from '../parseStackTrace';

const stackTrace = [
    '   at Ledger.Invoicing.PayInvoice.Handle(InvoiceId invoice) in /src/Invoicing/PayInvoice.cs:line 57',
    '--- End of stack trace from previous location ---',
    '   at Cratis.Arc.Commands.CommandPipeline.Execute(Object command)'
].join('\r\n');

describe('when parsing a dotnet stack trace', () => {
    const frames = parseStackTrace(stackTrace);

    it('should skip lines that are not frames', () => frames.length.should.equal(2));
    it('should have the method of the top frame', () => frames[0].method.should.equal('Ledger.Invoicing.PayInvoice.Handle(InvoiceId invoice)'));
    it('should have the file of the top frame', () => frames[0].file.should.equal('/src/Invoicing/PayInvoice.cs'));
    it('should have the line of the top frame', () => frames[0].line.should.equal(57));
    it('should have no file and line for a frame without symbols', () => [frames[1].method, frames[1].file, frames[1].line].should.deep.equal(['Cratis.Arc.Commands.CommandPipeline.Execute(Object command)', '', 0]));
});
