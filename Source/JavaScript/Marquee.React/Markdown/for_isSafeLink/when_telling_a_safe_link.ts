// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { isSafeLink, safeLinkOrEmpty } from '../safeLink.js';

describe('when telling a safe link', () => {
    it('should allow https', () => isSafeLink('https://example.com/status').should.be.true);
    it('should allow mailto', () => isSafeLink('mailto:support@example.com').should.be.true);
    it('should allow tel', () => isSafeLink('tel:+4712345678').should.be.true);
    it('should not allow plain http', () => isSafeLink('http://example.com').should.be.false);
    it('should not allow javascript', () => isSafeLink('javascript:alert(1)').should.be.false);
    it('should not allow javascript hidden by tabs and capitals', () => isSafeLink('Java\tScript:alert(1)').should.be.false);
    it('should not allow data', () => isSafeLink('data:text/html;base64,PHNjcmlwdD4=').should.be.false);
    it('should not allow a relative link', () => isSafeLink('/admin').should.be.false);
    it('should empty what is not safe', () => safeLinkOrEmpty('javascript:alert(1)').should.equal(''));
    it('should keep what is safe', () => safeLinkOrEmpty('https://example.com').should.equal('https://example.com'));
});
