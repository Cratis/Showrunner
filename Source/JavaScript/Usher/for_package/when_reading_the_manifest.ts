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
    it('should be the usher proxies package', () => manifest.name.should.equal('@cratis/showrunner.usher'));
    it('should not depend on a UI framework', () => Object.keys({ ...manifest.dependencies, ...manifest.peerDependencies }).should.not.include('react'));
});
