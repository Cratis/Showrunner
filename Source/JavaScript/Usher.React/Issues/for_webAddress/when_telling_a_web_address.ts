// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { isWebAddress } from '../webAddress.js';

describe('when telling a web address', () => {
    it('should accept an https address', () => isWebAddress('https://github.com/Cratis/Ledger/issues/42').should.be.true);
    it('should accept an http address', () => isWebAddress('http://tracker.example.com/42').should.be.true);
    it('should refuse a javascript address', () => isWebAddress('javascript:alert(1)').should.be.false);
    it('should refuse a data address', () => isWebAddress('data:text/html,hi').should.be.false);
    it('should refuse a relative address', () => isWebAddress('/issues/42').should.be.false);
    it('should refuse no address', () => isWebAddress(undefined).should.be.false);
});
