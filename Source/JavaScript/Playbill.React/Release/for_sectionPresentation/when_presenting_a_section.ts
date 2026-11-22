// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultPlaybillLabels, resolveLabels } from '../../Labels/index.js';
import { sectionPresentation } from '../sectionPresentation.js';

describe('when presenting a section', () => {
    it('should give a known section its style and text', () => sectionPresentation('Added', defaultPlaybillLabels).should.deep.equal({ title: 'Added', modifier: 'added' }));
    it('should not mind the case or the spacing of the heading', () => sectionPresentation('  FIXED ', defaultPlaybillLabels).modifier.should.equal('fixed'));
    it('should use the text a product gave', () => sectionPresentation('Fixed', resolveLabels({ fixed: 'Rettet' })).title.should.equal('Rettet'));
    it('should show another heading as it was written', () => sectionPresentation('Highlights', defaultPlaybillLabels).should.deep.equal({ title: 'Highlights', modifier: 'other' }));
});
