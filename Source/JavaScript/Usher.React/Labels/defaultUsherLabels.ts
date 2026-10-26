// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { UsherLabels } from './UsherLabels.js';

/**
 * The English labels of the Usher components.
 */
export const defaultUsherLabels: UsherLabels = {
    common: {
        cancel: 'Cancel',
        close: 'Close',
        sentWithYourReport: 'Sent with your report',
        recentRequests: '{count} recent requests',
        oneRecentRequest: '1 recent request',
        unknownLocation: 'Unknown location',
        locationSeparator: ' › '
    },
    reportButton: {
        reportProblem: 'Report a bug',
        reportError: 'Report this error'
    },
    myIssuesButton: {
        label: 'My issues',
        waiting: '{count} waiting for you'
    },
    access: {
        shareIdentity: 'Let support know who I am',
        shareIdentityDescription: 'Support sees your name and email. Without this, they only see where the report came from.',
        grantAccess: 'Let support sign in as me to look into this',
        grantAccessDescription: 'Only for this report, and only for the time you choose.',
        duration: 'For how long',
        oneHour: '1 hour',
        oneDay: '1 day',
        sevenDays: '7 days',
        notice: 'Support will see who you are and can act as you until {time}. You can revoke this at any time.'
    },
    reportIssue: {
        title: 'Report a problem',
        send: 'Send report',
        summary: 'Summary',
        description: 'What happened?'
    },
    reportError: {
        title: 'Report this error',
        send: 'Send report',
        technicalDetails: 'Technical details',
        errorType: 'Error type',
        correlationId: 'Correlation id',
        stackFrames: 'Stack frames',
        stackFrame: '{method} line {line}',
        details: 'What were you doing? (optional)',
        detailsPlaceholder: 'For example: I selected three invoices and pressed Pay selected.'
    },
    reportSent: {
        title: 'Report sent',
        view: 'View'
    },
    myIssues: {
        title: 'My issues',
        description: 'What you have reported. Updates appear here as support works on them.',
        reportProblem: 'Report a problem',
        showClosed: 'Show closed',
        issueColumn: 'Issue',
        stateColumn: 'State',
        lastActivityColumn: 'Last activity',
        waitingForYou: 'Waiting for you',
        fixedIn: 'Fixed in {version}',
        empty: 'You haven\'t reported anything yet.',
        noOpenIssues: 'You have no open issues.',
        loading: 'Loading your issues…',
        failed: 'Your issues could not be loaded.',
        unauthorized: 'You are not allowed to see issues.',
        errorKind: 'Error',
        manualKind: 'Problem'
    },
    states: {
        reported: 'Reported',
        triaged: 'Triaged',
        accepted: 'Accepted',
        inProgress: 'In progress',
        resolved: 'Resolved',
        wontFix: 'Won\'t fix',
        withdrawn: 'Withdrawn'
    },
    issue: {
        back: 'Back to my issues',
        reported: 'Reported {date}',
        knownIssue: 'Known issue:',
        trackedIn: 'Tracked in',
        notReleased: 'Not released yet',
        fixedIn: 'Fixed in {version}',
        questionBanner: 'Your reply answers support\'s question: “{question}”',
        waitingForAnswer: 'Waiting for your answer',
        answered: 'Answered',
        composerPlaceholder: 'Write to support',
        withdrawnPlaceholder: 'You withdrew this issue',
        withdrawnNotice: 'You withdrew this issue, so nothing more can be added to it.',
        notFound: 'This issue could not be found.',
        loading: 'Loading the issue…'
    },
    identity: {
        heading: 'Who you are',
        unknown: 'Support doesn\'t know who you are. They only see where the report came from.',
        known: 'Support knows who you are.',
        share: 'Share who I am',
        shareTitle: 'Share who you are',
        shareDescription: 'Support will see your name and email on this issue. This cannot be undone.',
        shareConfirm: 'Share'
    },
    accessPanel: {
        grantedHeading: 'Support can sign in as you',
        notGrantedHeading: 'Support access',
        requestedHeading: 'Support asks to sign in as you',
        grantedUntil: 'Until {time}.',
        notGranted: 'Support cannot sign in as you.',
        requestReason: '“{reason}” Granting also shares who you are.',
        grant: 'Grant access',
        notNow: 'Not now',
        revoke: 'Revoke access',
        revokeTitle: 'Revoke access?',
        revokeMessage: 'Support will no longer be able to sign in as you on this issue.',
        grantTitle: 'Let support sign in as you',
        grantConfirm: 'Grant access',
        noSessions: 'No support sessions yet.',
        session: '{name} signed in as you · {start}–{end}',
        sessionRunning: 'now',
        sessionsHeading: 'Support sessions',
        sessionDidNothing: 'Did nothing as you.',
        sessionActions: {
            detailsAdded: 'Added details: {text}',
            questionAnswered: 'Answered a question: {text}',
            identityShared: 'Shared who you are',
            reopened: 'Reopened the issue: {text}',
            withdrawn: 'Withdrew the issue',
            accessRevoked: 'Revoked its access'
        }
    },
    actions: {
        reopen: 'Reopen issue',
        reopenTitle: 'Reopen issue',
        reopenReason: 'Why is it not fixed?',
        withdraw: 'Withdraw issue',
        withdrawTitle: 'Withdraw this issue?',
        withdrawMessage: 'Support stops working on it, and you cannot add to it afterwards.'
    },
    conversation: {
        you: 'You',
        support: 'Support',
        update: 'Update',
        empty: 'Nothing has happened on this issue yet.',
        send: 'Send',
        justNow: 'just now',
        minutesAgo: '{minutes} min ago',
        hoursAgo: '{hours} h ago',
        events: {
            identityShared: 'You shared who you are',
            accessGranted: 'You let support sign in as you',
            accessRevoked: 'You revoked support access',
            accessLapsed: 'Support access ended',
            accessRequested: 'Support asked to sign in as you: {text}',
            supportSignedIn: 'Support signed in as you',
            supportSessionEnded: 'Support signed out',
            triaged: 'Triaged by support',
            accepted: 'Accepted',
            declinedToFix: 'Won\'t fix: {text}',
            workStarted: 'Work started',
            resolved: 'Resolved: {text}',
            reopened: 'You reopened the issue: {text}',
            withdrawn: 'You withdrew the issue',
            linkedToKnownIssue: 'Linked to known issue “{text}”',
            unlinkedFromKnownIssue: 'No longer linked to a known issue',
            linkedToWorkItem: 'Tracked as “{text}”',
            unlinkedFromWorkItem: 'No longer tracked',
            fixReleased: 'Fixed in {text}'
        }
    }
};
