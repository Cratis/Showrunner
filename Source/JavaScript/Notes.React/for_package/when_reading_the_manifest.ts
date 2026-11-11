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
    it('should need an arc release that has the application context', () => (manifest.peerDependencies ?? {})['@cratis/arc.react'].should.equal('^22.185.1'));
    it('should not point at a local file', () => JSON.stringify(manifest).should.not.contain('file:'));
});
