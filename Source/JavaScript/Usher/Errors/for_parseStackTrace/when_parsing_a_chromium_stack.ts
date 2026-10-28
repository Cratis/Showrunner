// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { parseStackTrace } from '../parseStackTrace';

const stack = [
    'TypeError: Cannot read properties of undefined (reading \'total\')',
    '    at InvoiceSummary.render (https://ledger.example.com/assets/index-4f1c.js:120:17)',
    '    at async payInvoice (http://localhost:5173/src/Invoicing/Pay.tsx:42:9)',
    '    at https://ledger.example.com/assets/vendor-9ab2.js:3:1',
    '    at <anonymous>'
].join('\n');

describe('when parsing a chromium stack', () => {
    const frames = parseStackTrace(stack);

    it('should skip the message line', () => frames.length.should.equal(4));
    it('should have the method of the top frame', () => frames[0].method.should.equal('InvoiceSummary.render'));
    it('should have the file of the top frame', () => frames[0].file.should.equal('https://ledger.example.com/assets/index-4f1c.js'));
    it('should have the line of the top frame', () => frames[0].line.should.equal(120));
    it('should leave out async', () => frames[1].method.should.equal('payInvoice'));
    it('should keep the port in the file', () => frames[1].file.should.equal('http://localhost:5173/src/Invoicing/Pay.tsx'));
    it('should have an empty method for an anonymous location', () => frames[2].method.should.equal(''));
    it('should have the file of an anonymous location', () => frames[2].file.should.equal('https://ledger.example.com/assets/vendor-9ab2.js'));
    it('should have no file and line for a frame without a location', () => [frames[3].method, frames[3].file, frames[3].line].should.deep.equal(['<anonymous>', '', 0]));
});
