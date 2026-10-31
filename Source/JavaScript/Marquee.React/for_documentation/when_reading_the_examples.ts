// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readdirSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const repository = join(here, '..', '..', '..', '..');
const documentation = join(repository, 'Documentation', 'Marquee');
const exampleFolders = [join(here, '..', 'Examples'), join(here, '..', '..', 'Marquee.Authoring', 'Examples')];

const licenseHeaderLines = 3;
const body = (path: string): string => readFileSync(path, 'utf-8').split('\n').slice(licenseHeaderLines).join('\n').trim();

const pages = readdirSync(documentation)
    .filter(file => file.endsWith('.md'))
    .map(file => readFileSync(join(documentation, file), 'utf-8'));

const examples = exampleFolders.flatMap(folder => readdirSync(folder)
    .filter(file => /\.tsx?$/.test(file))
    .map(file => ({ file, body: body(join(folder, file)) })));

describe('when reading the documentation examples', () => {
    it('should have found the examples', () => examples.length.should.be.greaterThan(5));

    for (const example of examples) {
        it(`should show ${example.file} in the documentation exactly as it builds`, () => pages.some(page => page.includes(example.body)).should.be.true);
    }
});
