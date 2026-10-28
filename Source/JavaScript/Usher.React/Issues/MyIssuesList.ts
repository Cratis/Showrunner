// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReportedIssue } from '@cratis/showrunner.usher';
import { isClosed } from './issueStates.js';

/**
 * Counts the issues support waits for the reporter on.
 * @param issues The reporter's issues.
 * @returns How many issues support waits for the reporter on.
 */
export function waitingCount(issues: ReportedIssue[]): number {
    return issues.filter(issue => issue.waitingOnReporter).length;
}

/**
 * Gets the issues to list.
 * @param issues The reporter's issues, most recent activity first.
 * @param showClosed Whether to list closed issues too.
 * @returns The issues to list, in the order given.
 */
export function visibleIssues(issues: ReportedIssue[], showClosed: boolean): ReportedIssue[] {
    return showClosed ? issues : issues.filter(issue => !isClosed(issue.state));
}

/**
 * Checks whether a release has the fix for an issue.
 * @param issue The issue.
 * @returns True when a release has the fix.
 */
export function isFixReleased(issue: { fixedInVersion: string }): boolean {
    return typeof issue.fixedInVersion === 'string' && issue.fixedInVersion.length > 0;
}
