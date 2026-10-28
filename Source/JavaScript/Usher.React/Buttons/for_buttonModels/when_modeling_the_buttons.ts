// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultUsherLabels } from '../../Labels/defaultUsherLabels.js';
import { myIssuesButtonModel, reportButtonModel } from '../buttonModels.js';

describe('when modeling the buttons', () => {
    const largeReport = reportButtonModel('large', false, defaultUsherLabels);
    const smallReport = reportButtonModel('small', false, defaultUsherLabels);
    const smallError = reportButtonModel('small', true, defaultUsherLabels);
    const smallIssues = myIssuesButtonModel('small', 1, defaultUsherLabels);
    const largeIssues = myIssuesButtonModel('large', 0, defaultUsherLabels);

    it('should show the text on a large button', () => largeReport.text.should.equal('Report a bug'));
    it('should have no tooltip on a large button', () => largeReport.tooltip.should.equal(''));
    it('should show no text on a small button', () => smallReport.text.should.equal(''));
    it('should name a small button', () => smallReport.accessibleName.should.equal('Report a bug'));
    it('should give a small button a tooltip', () => smallReport.tooltip.should.equal('Report a bug'));
    it('should name the report button for an error', () => smallError.accessibleName.should.equal('Report this error'));
    it('should say how many wait in the name', () => smallIssues.accessibleName.should.equal('My issues, 1 waiting for you'));
    it('should not mention waiting when none wait', () => largeIssues.accessibleName.should.equal('My issues'));
    it('should show the text on a large my issues button', () => largeIssues.text.should.equal('My issues'));
});
