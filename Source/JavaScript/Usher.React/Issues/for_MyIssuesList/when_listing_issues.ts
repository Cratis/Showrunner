// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { IssueState } from '@cratis/showrunner.usher';
import { a_reported_issue } from '../given/an_issue.js';
import { isFixReleased, visibleIssues, waitingCount } from '../MyIssuesList.js';

describe('when listing issues', () => {
    const waiting = a_reported_issue({ state: IssueState.triaged, waitingOnReporter: true });
    const alsoWaiting = a_reported_issue({ state: IssueState.inProgress, waitingOnReporter: true });
    const open = a_reported_issue({ state: IssueState.accepted });
    const resolved = a_reported_issue({ state: IssueState.resolved, fixedInVersion: '4.12.3' });
    const withdrawn = a_reported_issue({ state: IssueState.withdrawn });
    const issues = [waiting, open, resolved, alsoWaiting, withdrawn];

    it('should count the issues support waits for the reporter on', () => waitingCount(issues).should.equal(2));
    it('should count none when there are no issues', () => waitingCount([]).should.equal(0));
    it('should hide closed issues', () => visibleIssues(issues, false).should.deep.equal([waiting, open, alsoWaiting]));
    it('should show closed issues when asked', () => visibleIssues(issues, true).should.deep.equal(issues));
    it('should know a release has the fix', () => isFixReleased(resolved).should.be.true);
    it('should know no release has the fix yet', () => isFixReleased(open).should.be.false);
});
