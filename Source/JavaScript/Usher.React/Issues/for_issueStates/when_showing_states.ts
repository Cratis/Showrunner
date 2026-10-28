// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { IssueState } from '@cratis/showrunner.usher';
import { defaultUsherLabels } from '../../Labels/defaultUsherLabels.js';
import { isClosed, stateBadgeFor, statusStepOf } from '../issueStates.js';

describe('when showing states', () => {
    it('should show reported as secondary', () => stateBadgeFor(IssueState.reported, defaultUsherLabels).should.deep.equal({ label: 'Reported', severity: 'secondary' }));
    it('should show triaged as info', () => stateBadgeFor(IssueState.triaged, defaultUsherLabels).should.deep.equal({ label: 'Triaged', severity: 'info' }));
    it('should show accepted as info', () => stateBadgeFor(IssueState.accepted, defaultUsherLabels).should.deep.equal({ label: 'Accepted', severity: 'info' }));
    it('should show in progress as warn', () => stateBadgeFor(IssueState.inProgress, defaultUsherLabels).should.deep.equal({ label: 'In progress', severity: 'warn' }));
    it('should show resolved as success', () => stateBadgeFor(IssueState.resolved, defaultUsherLabels).should.deep.equal({ label: 'Resolved', severity: 'success' }));
    it('should show won\'t fix as contrast', () => stateBadgeFor(IssueState.wontFix, defaultUsherLabels).should.deep.equal({ label: 'Won\'t fix', severity: 'contrast' }));
    it('should show withdrawn as secondary', () => stateBadgeFor(IssueState.withdrawn, defaultUsherLabels).should.deep.equal({ label: 'Withdrawn', severity: 'secondary' }));
    it('should take the label from the labels given', () => stateBadgeFor(IssueState.resolved, { ...defaultUsherLabels, states: { ...defaultUsherLabels.states, resolved: 'Løst' } }).label.should.equal('Løst'));
    it('should count resolved as closed', () => isClosed(IssueState.resolved).should.be.true);
    it('should count won\'t fix as closed', () => isClosed(IssueState.wontFix).should.be.true);
    it('should count withdrawn as closed', () => isClosed(IssueState.withdrawn).should.be.true);
    it('should count in progress as open', () => isClosed(IssueState.inProgress).should.be.false);
    it('should place triaged on the second step', () => statusStepOf(IssueState.triaged).should.equal(1));
    it('should place withdrawn on the last step', () => statusStepOf(IssueState.withdrawn).should.equal(4));
});
