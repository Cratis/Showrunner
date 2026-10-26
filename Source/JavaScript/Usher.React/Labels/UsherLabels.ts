// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Every text the Usher components show or announce. Placeholders in braces, such as `{time}`, are filled in by the
 * components.
 */
export interface UsherLabels {
    /** Texts shared by several components. */
    common: {
        /** The cancel button. */
        cancel: string;
        /** The accessible name of a dialog's close button. */
        close: string;
        /** Introduces what is sent with a report. */
        sentWithYourReport: string;
        /** How many recent requests are sent with a report. `{count}` is the number. */
        recentRequests: string;
        /** Shown for one recent request sent with a report. */
        oneRecentRequest: string;
        /** Shown when nothing is known about where the user is. */
        unknownLocation: string;
        /** Separates the module, feature and slice of a location. */
        locationSeparator: string;
    };

    /** The report button. */
    reportButton: {
        /** The text, tooltip and accessible name of the button that reports a problem. */
        reportProblem: string;
        /** The text, tooltip and accessible name of the button that reports an error. */
        reportError: string;
    };

    /** The my issues button. */
    myIssuesButton: {
        /** The text, tooltip and accessible name of the button. */
        label: string;
        /** Added to the accessible name when support waits for the user. `{count}` is the number of issues. */
        waiting: string;
    };

    /** The identity and access choices of the report dialogs and the grant access dialog. */
    access: {
        /** The checkbox that shares who the reporter is. */
        shareIdentity: string;
        /** Explains what sharing who the reporter is means. */
        shareIdentityDescription: string;
        /** The switch that lets support sign in as the reporter. */
        grantAccess: string;
        /** Explains what letting support sign in means. */
        grantAccessDescription: string;
        /** The duration dropdown. */
        duration: string;
        /** One hour of access. */
        oneHour: string;
        /** One day of access. */
        oneDay: string;
        /** Seven days of access. */
        sevenDays: string;
        /** States what granting access means. `{time}` is when it ends. */
        notice: string;
    };

    /** The report a problem dialog. */
    reportIssue: {
        /** The title. */
        title: string;
        /** The button that sends the report. */
        send: string;
        /** The summary field. */
        summary: string;
        /** The description field. */
        description: string;
    };

    /** The report this error dialog. */
    reportError: {
        /** The title. */
        title: string;
        /** The button that sends the report. */
        send: string;
        /** The collapsed section with the technical details. */
        technicalDetails: string;
        /** The type of the error. */
        errorType: string;
        /** The correlation id of the error. */
        correlationId: string;
        /** The stack frames of the error. */
        stackFrames: string;
        /** One stack frame. `{method}`, `{file}` and `{line}` are the frame's parts. */
        stackFrame: string;
        /** The details field. */
        details: string;
        /** The placeholder of the details field. */
        detailsPlaceholder: string;
    };

    /** The notification shown when a report is sent. */
    reportSent: {
        /** The title. */
        title: string;
        /** The action that opens the issue. */
        view: string;
    };

    /** The my issues side sheet. */
    myIssues: {
        /** The title. */
        title: string;
        /** The subtitle. */
        description: string;
        /** The button that reports a problem. */
        reportProblem: string;
        /** The switch that shows closed issues. */
        showClosed: string;
        /** The issue column. */
        issueColumn: string;
        /** The state column. */
        stateColumn: string;
        /** The last activity column. */
        lastActivityColumn: string;
        /** Marks an issue support waits for the user on. */
        waitingForYou: string;
        /** Marks an issue a release fixed. `{version}` is the version. */
        fixedIn: string;
        /** Shown when the user has reported nothing. */
        empty: string;
        /** Shown when only closed issues exist and they are hidden. */
        noOpenIssues: string;
        /** Shown while the issues load. */
        loading: string;
        /** Shown when the issues could not load. */
        failed: string;
        /** Shown when the user may not see issues. */
        unauthorized: string;
        /** An issue reported from an error. */
        errorKind: string;
        /** An issue described by hand. */
        manualKind: string;
    };

    /** The states of an issue. */
    states: {
        /** Reported. */
        reported: string;
        /** Triaged. */
        triaged: string;
        /** Accepted. */
        accepted: string;
        /** In progress. */
        inProgress: string;
        /** Resolved. */
        resolved: string;
        /** Won't fix. */
        wontFix: string;
        /** Withdrawn. */
        withdrawn: string;
    };

    /** The issue view. */
    issue: {
        /** The back button. */
        back: string;
        /** When the issue was reported. `{date}` is the date. */
        reported: string;
        /** Introduces the known issue. */
        knownIssue: string;
        /** Introduces the work item. */
        trackedIn: string;
        /** Shown while no release has the fix. */
        notReleased: string;
        /** Shown once a release has the fix. `{version}` is the version. */
        fixedIn: string;
        /** Quotes the question support waits for an answer to. `{question}` is the question. */
        questionBanner: string;
        /** Marks the question support waits for an answer to. */
        waitingForAnswer: string;
        /** Marks an answered question. */
        answered: string;
        /** The placeholder of the composer. */
        composerPlaceholder: string;
        /** The placeholder of the composer once the issue is withdrawn. */
        withdrawnPlaceholder: string;
        /** Shown when a message is sent on a withdrawn issue. */
        withdrawnNotice: string;
        /** Shown when the issue cannot be found. */
        notFound: string;
        /** Shown while the issue loads. */
        loading: string;
    };

    /** The identity panel of the issue view. */
    identity: {
        /** The heading. */
        heading: string;
        /** Shown when support does not know who the reporter is. */
        unknown: string;
        /** Shown when support knows who the reporter is. */
        known: string;
        /** The button that shares who the reporter is. */
        share: string;
        /** The title of the share dialog. */
        shareTitle: string;
        /** Explains what is shared. */
        shareDescription: string;
        /** The confirm button of the share dialog. */
        shareConfirm: string;
    };

    /** The access panel of the issue view. */
    accessPanel: {
        /** The heading while support may sign in. */
        grantedHeading: string;
        /** The heading while support may not sign in. */
        notGrantedHeading: string;
        /** The heading while support asks to sign in. */
        requestedHeading: string;
        /** Until when support may sign in. `{time}` is when it ends. */
        grantedUntil: string;
        /** Shown when support may not sign in. */
        notGranted: string;
        /** Quotes support's reason for asking. `{reason}` is the reason. */
        requestReason: string;
        /** The button that grants access. */
        grant: string;
        /** The button that dismisses support's request for now. */
        notNow: string;
        /** The button that revokes access. */
        revoke: string;
        /** The title of the revoke confirmation. */
        revokeTitle: string;
        /** The message of the revoke confirmation. */
        revokeMessage: string;
        /** The title of the grant dialog. */
        grantTitle: string;
        /** The confirm button of the grant dialog. */
        grantConfirm: string;
        /** Shown when support has not signed in. */
        noSessions: string;
        /** One session. `{name}`, `{start}` and `{end}` are filled in. */
        session: string;
        /** The end of a session still running. */
        sessionRunning: string;
        /** The heading of the sessions list. */
        sessionsHeading: string;
    };

    /** The actions of the issue view. */
    actions: {
        /** The button that reopens a resolved issue. */
        reopen: string;
        /** The title of the reopen dialog. */
        reopenTitle: string;
        /** The reason field of the reopen dialog. */
        reopenReason: string;
        /** The button that withdraws the issue. */
        withdraw: string;
        /** The title of the withdraw confirmation. */
        withdrawTitle: string;
        /** The message of the withdraw confirmation. */
        withdrawMessage: string;
    };

    /** The conversation of the issue view. */
    conversation: {
        /** The reporter. */
        you: string;
        /** Support, when the desk hides the name. */
        support: string;
        /** The author of state changes. */
        update: string;
        /** Shown when there is nothing in the conversation. */
        empty: string;
        /** The accessible name of the send button. */
        send: string;
        /** Under a minute ago. */
        justNow: string;
        /** Minutes ago. `{minutes}` is the number. */
        minutesAgo: string;
        /** Hours ago. `{hours}` is the number. */
        hoursAgo: string;
        /** What a state change shows, by entry kind. `{text}` is the entry's text. */
        events: ConversationEventLabels;
    };
}

/**
 * What the state changes in a conversation show. `{text}` is the text of the entry, such as a reason or a version.
 */
export interface ConversationEventLabels {
    /** The reporter shared who they are. */
    identityShared: string;
    /** The reporter let support sign in. */
    accessGranted: string;
    /** The reporter revoked access. */
    accessRevoked: string;
    /** Access ended. */
    accessLapsed: string;
    /** Support asked to sign in. */
    accessRequested: string;
    /** Support signed in. */
    supportSignedIn: string;
    /** A support session ended. */
    supportSessionEnded: string;
    /** Support triaged the issue. */
    triaged: string;
    /** Support accepted the issue. */
    accepted: string;
    /** Support declined to fix the issue. */
    declinedToFix: string;
    /** Support started work. */
    workStarted: string;
    /** Support resolved the issue. */
    resolved: string;
    /** The reporter reopened the issue. */
    reopened: string;
    /** The reporter withdrew the issue. */
    withdrawn: string;
    /** The issue was linked to a known issue. */
    linkedToKnownIssue: string;
    /** The issue was unlinked from its known issue. */
    unlinkedFromKnownIssue: string;
    /** The issue was linked to a work item. */
    linkedToWorkItem: string;
    /** The issue was unlinked from its work item. */
    unlinkedFromWorkItem: string;
    /** A release with the fix shipped. */
    fixReleased: string;
}

/**
 * Makes every property of a type optional, all the way down.
 */
export type DeepPartial<T> = {
    [TKey in keyof T]?: T[TKey] extends object ? DeepPartial<T[TKey]> : T[TKey];
};

/**
 * The labels of the Usher components that a solution replaces, for instance to localize them. Labels left out keep
 * their English defaults.
 */
export type UsherLabelOverrides = DeepPartial<UsherLabels>;
