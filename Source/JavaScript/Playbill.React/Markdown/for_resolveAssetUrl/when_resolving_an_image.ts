// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { resolveAssetUrl } from '../assetUrl.js';

describe('when resolving an image', () => {
    const base = '/.cratis/playbill/v1/releases/2.3.0';

    it('should load a captured file from the folder of the release', () => resolveAssetUrl('hero.png', base).should.equal(`${base}/hero.png`));
    it('should ignore a leading dot and slash', () => resolveAssetUrl('./hero.png', base).should.equal(`${base}/hero.png`));
    it('should encode the name of the file', () => resolveAssetUrl('a b.png', base).should.equal(`${base}/a%20b.png`));
    it('should keep a folder inside the release', () => resolveAssetUrl('shots/hero.png', base).should.equal(`${base}/shots/hero.png`));
    it('should leave an https image as it was written', () => resolveAssetUrl('https://example.com/hero.png', base).should.equal('https://example.com/hero.png'));
    it('should refuse a path that climbs out of the folder', () => resolveAssetUrl('../1.0.0/hero.png', base).should.equal(''));
    it('should refuse an absolute path', () => resolveAssetUrl('/secret.png', base).should.equal(''));
    it('should refuse a protocol-relative address', () => resolveAssetUrl('//example.com/hero.png', base).should.equal(''));
    it('should refuse a script', () => resolveAssetUrl('javascript:alert(1)', base).should.equal(''));
    it('should refuse an inline image', () => resolveAssetUrl('data:image/png;base64,AAAA', base).should.equal(''));
    it('should refuse an http image', () => resolveAssetUrl('http://example.com/hero.png', base).should.equal(''));
    it('should refuse a relative image when the folder is not known', () => resolveAssetUrl('hero.png').should.equal(''));
});
