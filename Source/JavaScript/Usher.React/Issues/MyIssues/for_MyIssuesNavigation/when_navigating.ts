// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { navigate, startNavigation } from '../MyIssuesNavigation.js';

describe('when navigating', () => {
    const issueId = Guid.create();
    const otherIssueId = Guid.create();
    const onList = startNavigation();
    const onIssue = navigate(onList, { type: 'opened', issueId });
    const back = navigate(onIssue, { type: 'back' });
    const showingClosed = navigate(onList, { type: 'showClosedChanged', showClosed: true });
    const requestedOnIssue = startNavigation(issueId);
    const requestedAgain = navigate(navigate(requestedOnIssue, { type: 'back' }), { type: 'requested', issueId: Guid.parse(issueId.toString()) });
    const requestedOther = navigate(requestedOnIssue, { type: 'requested', issueId: otherIssueId });

    it('should start on the list', () => (onList.openIssueId === undefined).should.be.true);
    it('should start hiding closed issues', () => onList.showClosed.should.be.false);
    it('should open an issue', () => onIssue.openIssueId!.should.equal(issueId));
    it('should go back to the list', () => (back.openIssueId === undefined).should.be.true);
    it('should show closed issues when asked', () => showingClosed.showClosed.should.be.true);
    it('should start on the issue asked for', () => requestedOnIssue.openIssueId!.should.equal(issueId));
    it('should stay on the list when the same issue is asked for again', () => (requestedAgain.openIssueId === undefined).should.be.true);
    it('should move to another issue asked for', () => requestedOther.openIssueId!.should.equal(otherIssueId));
});
