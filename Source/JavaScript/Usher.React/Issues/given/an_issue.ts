// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { AccessRequest, AccessState, ConversationAuthorKind, ConversationEntry, ConversationEntryKind, IssueAccess, IssueKind, IssueState, ReportedIssue } from '@cratis/showrunner.usher';

/**
 * Builds a reported issue for a specification.
 * @param values The values that differ from an open, manually reported issue nobody waits on.
 * @returns The {@link ReportedIssue}.
 */
export function a_reported_issue(values: Partial<ReportedIssue> = {}): ReportedIssue {
    return Object.assign(new ReportedIssue(), {
        id: Guid.create(),
        kind: IssueKind.manual,
        title: 'The pay button does nothing',
        state: IssueState.reported,
        reportedAt: new Date(Date.UTC(2026, 9, 26, 12, 0)),
        lastActivityAt: new Date(Date.UTC(2026, 9, 26, 12, 0)),
        waitingOnReporter: false,
        knownIssueTitle: '',
        fixedInVersion: ''
    }, values);
}

/**
 * Builds a conversation entry for a specification.
 * @param values The values that differ from a description by the reporter.
 * @returns The {@link ConversationEntry}.
 */
export function an_entry(values: Partial<ConversationEntry> = {}): ConversationEntry {
    return Object.assign(new ConversationEntry(), {
        entryId: 1,
        kind: ConversationEntryKind.issueDescribed,
        text: 'The pay button does nothing',
        authorKind: ConversationAuthorKind.reporter,
        authorName: '',
        at: new Date(Date.UTC(2026, 9, 26, 12, 0)),
        questionId: Guid.empty
    }, values);
}

/**
 * Builds the access of an issue for a specification.
 * @param values The values that differ from no access and no request.
 * @param pendingRequest The reason of a pending request; none when not given.
 * @returns The {@link IssueAccess}.
 */
export function access(values: Partial<IssueAccess> = {}, pendingRequest?: string): IssueAccess {
    return Object.assign(new IssueAccess(), {
        state: AccessState.notGranted,
        validUntil: new Date(0),
        request: Object.assign(new AccessRequest(), { pending: pendingRequest !== undefined, reason: pendingRequest ?? '' }),
        sessions: []
    }, values);
}
