// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { safeUrl } from '../safeUrl.js';

describe('when judging an address', () => {
    it('should keep a secure address', () => safeUrl('https://example.com/a').should.equal('https://example.com/a'));
    it('should keep a mail address', () => safeUrl('mailto:someone@example.com').should.equal('mailto:someone@example.com'));
    it('should keep a relative path', () => safeUrl('media/picture.png').should.equal('media/picture.png'));
    it('should keep a path from the root', () => safeUrl('/assets/picture.png').should.equal('/assets/picture.png'));
    it('should keep an anchor', () => safeUrl('#chapter').should.equal('#chapter'));
    it('should keep an inline image', () => safeUrl('data:image/png;base64,AAAA').should.equal('data:image/png;base64,AAAA'));
    it('should refuse script', () => safeUrl('javascript:alert(1)').should.equal(''));
    it('should refuse script in any case', () => safeUrl('JaVaScRiPt:alert(1)').should.equal(''));
    it('should refuse an inline page', () => safeUrl('data:text/html;base64,AAAA').should.equal(''));
    it('should refuse nothing at all', () => safeUrl('  ').should.equal(''));
});
