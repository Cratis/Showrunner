// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';

/**
 * Where the my issues side sheet is: the list, or one issue.
 */
export interface MyIssuesNavigation {
    /** The issue shown; the list when not set. */
    readonly openIssueId?: Guid;

    /** The issue the side sheet was last asked to open on, so a new request moves to it. */
    readonly requestedIssueId?: Guid;

    /** Whether the list shows closed issues too. */
    readonly showClosed: boolean;
}

/**
 * Something that moves the my issues side sheet.
 */
export type MyIssuesNavigationAction =
    | { readonly type: 'requested'; readonly issueId?: Guid }
    | { readonly type: 'opened'; readonly issueId: Guid }
    | { readonly type: 'back' }
    | { readonly type: 'showClosedChanged'; readonly showClosed: boolean };

/**
 * Gets where the side sheet starts: on the issue asked for, or on the list of open issues.
 * @param issueId The issue to open on; the list when not given.
 * @returns The {@link MyIssuesNavigation}.
 */
export function startNavigation(issueId?: Guid): MyIssuesNavigation {
    return { openIssueId: issueId, requestedIssueId: issueId, showClosed: false };
}

/**
 * Moves the side sheet.
 * @param navigation Where the side sheet is.
 * @param action What moves it.
 * @returns Where it is after.
 */
export function navigate(navigation: MyIssuesNavigation, action: MyIssuesNavigationAction): MyIssuesNavigation {
    switch (action.type) {
        case 'requested':
            return sameIssue(navigation.requestedIssueId, action.issueId)
                ? navigation
                : { ...navigation, openIssueId: action.issueId, requestedIssueId: action.issueId };
        case 'opened':
            return { ...navigation, openIssueId: action.issueId };
        case 'back':
            return { ...navigation, openIssueId: undefined };
        case 'showClosedChanged':
            return { ...navigation, showClosed: action.showClosed };
    }
}

function sameIssue(left?: Guid, right?: Guid): boolean {
    return left === right || (left !== undefined && right !== undefined && left.equals(right));
}
