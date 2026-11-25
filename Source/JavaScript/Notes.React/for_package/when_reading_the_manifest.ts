// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const packageRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const manifest = JSON.parse(readFileSync(join(packageRoot, 'package.json'), 'utf-8')) as {
    name: string;
    dependencies?: Record<string, string>;
    peerDependencies?: Record<string, string>;
};

describe('when reading the manifest', () => {
    it('should be the notes react package', () => manifest.name.should.equal('@cratis/showrunner.notes.react'));
    it('should depend on the notes proxies package', () => (manifest.dependencies ?? {}).should.have.property('@cratis/showrunner.notes'));
    it('should need a components release that has the component picker', () => (manifest.peerDependencies ?? {})['@cratis/components'].should.equal('^4.39.0'));
    it('should need an arc release that has the application context and the part identity the proxies carry', () => (manifest.peerDependencies ?? {})['@cratis/arc.react'].should.equal('^24.8.0'));
    it('should not accept an arc release before the part identity', () => (manifest.peerDependencies ?? {})['@cratis/arc.react'].should.not.contain('^24.0.0'));
    it('should accept the same arc releases for the arc core package', () => (manifest.peerDependencies ?? {})['@cratis/arc'].should.equal((manifest.peerDependencies ?? {})['@cratis/arc.react']));
    it('should not point at a local file', () => JSON.stringify(manifest).should.not.contain('file:'));
});
