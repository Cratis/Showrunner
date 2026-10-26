// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { parseStackTrace } from '../parseStackTrace';

const stack = [
    'payInvoice@https://ledger.example.com/assets/index-4f1c.js:42:9',
    '@https://ledger.example.com/assets/index-4f1c.js:7:1'
].join('\n');

describe('when parsing a firefox or safari stack', () => {
    const frames = parseStackTrace(stack);

    it('should have both frames', () => frames.length.should.equal(2));
    it('should have the method of the top frame', () => frames[0].method.should.equal('payInvoice'));
    it('should have the file of the top frame', () => frames[0].file.should.equal('https://ledger.example.com/assets/index-4f1c.js'));
    it('should have the line of the top frame', () => frames[0].line.should.equal(42));
    it('should have an empty method for an anonymous frame', () => frames[1].method.should.equal(''));
});
