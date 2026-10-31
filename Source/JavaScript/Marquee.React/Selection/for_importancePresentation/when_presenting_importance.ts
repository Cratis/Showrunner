// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Importance } from '@cratis/showrunner.marquee';
import { defaultMarqueeLabels } from '../../Labels/index.js';
import { importancePresentation } from '../importancePresentation.js';

describe('when presenting importance', () => {
    it('should present information', () => importancePresentation(Importance.information, defaultMarqueeLabels).should.deep.equal({ modifier: 'information', label: 'Information' }));
    it('should present a warning', () => importancePresentation(Importance.warning, defaultMarqueeLabels).should.deep.equal({ modifier: 'warning', label: 'Warning' }));
    it('should present a critical announcement', () => importancePresentation(Importance.critical, defaultMarqueeLabels).should.deep.equal({ modifier: 'critical', label: 'Critical' }));
    it('should name the icon in the labels given', () => importancePresentation(Importance.warning, { ...defaultMarqueeLabels, warning: 'Advarsel' }).label.should.equal('Advarsel'));
});
