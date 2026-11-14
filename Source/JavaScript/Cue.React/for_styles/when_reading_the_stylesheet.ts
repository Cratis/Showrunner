// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const stylesheet = readFileSync(join(here, '..', 'styles.css'), 'utf-8');

// A --cratis-surface-* token is a background: in the dark theme surface-0 is nearly black, so text drawn in it cannot be read
// on the dark cards the tour puts it on (the subtitles were exactly that).
const textColours = [...stylesheet.matchAll(/^\s*color:\s*(var\(--cratis-[a-z0-9-]+\))/gm)].map(match => match[1]);

describe('when reading the stylesheet', () => {
    it('should have found the text colours', () => textColours.length.should.be.greaterThan(10));
    it('should not draw any text in a surface colour', () => textColours.filter(colour => colour.includes('surface')).should.be.empty);
});
