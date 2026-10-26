// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const packageRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const manifest = JSON.parse(readFileSync(join(packageRoot, 'package.json'), 'utf-8')) as {
    name: string;
    dependencies?: Record<string, string>;
};

describe('when reading the manifest', () => {
    it('should be the usher react package', () => manifest.name.should.equal('@cratis/showrunner.usher.react'));
    it('should depend on the usher proxies package', () => (manifest.dependencies ?? {}).should.have.property('@cratis/showrunner.usher'));
});
