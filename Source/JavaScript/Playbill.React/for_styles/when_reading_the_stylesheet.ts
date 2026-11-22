// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const stylesheet = readFileSync(join(here, '..', 'styles.css'), 'utf-8');
const classesInComponents = [
    'playbill', 'playbill__list', 'playbill__content', 'playbill-item', 'playbill-item--selected', 'playbill-item__version',
    'playbill-release', 'playbill-release__header', 'playbill-release__version', 'playbill-release__lead', 'playbill-section',
    'playbill-section__title', 'playbill-tag', 'playbill-tag--current', 'playbill-tag--unread', 'playbill-markdown',
    'playbill-dialog', 'playbill-dialog__earlier', 'playbill-status', 'playbill-status--failed', 'playbill-badge', 'playbill-badge__count'
];
const sections = ['added', 'changed', 'fixed', 'removed', 'security', 'deprecated'];

describe('when reading the stylesheet', () => {
    it('should style every part the components draw', () => classesInComponents.filter(name => !stylesheet.includes(`.${name}`)).should.be.empty);
    it('should mark every section the release notes use', () => sections.filter(name => !stylesheet.includes(`.playbill-section--${name}`)).should.be.empty);
    it('should not draw any text in a surface colour', () => [...stylesheet.matchAll(/^\s*color:\s*(var\(--[a-z0-9-]+)/gm)].map(match => match[1]).filter(colour => colour.includes('surface')).should.be.empty);
    it('should let a product restyle it with its own properties', () => ['--playbill-accent', '--playbill-border', '--playbill-text', '--playbill-muted', '--playbill-radius'].filter(name => !stylesheet.includes(name)).should.be.empty);
});
