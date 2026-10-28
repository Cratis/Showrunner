// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcContext } from '@cratis/arc.react';
import { DialogResult, useDialog } from '@cratis/arc.react/dialogs';
import { SliceLocation } from '@cratis/arc/reflection';
import { toast } from '@cratis/components/Notifications';
import { Guid } from '@cratis/fundamentals';
import { ReactNode, useContext, useMemo, useRef } from 'react';
import { CorrelationTracker, defaultCorrelationIdHeader, withCorrelationIdHeader } from '../Correlation/index.js';
import { MyIssuesDialog } from '../Issues/MyIssues/MyIssuesDialog.js';
import { resolveLabels, UsherLabelOverrides } from '../Labels/index.js';
import { ReportErrorDialog } from '../Reporting/ReportErrorDialog.js';
import { ReportIssueDialog } from '../Reporting/ReportIssueDialog.js';
import { ReportSent } from '../Reporting/ReportSent.js';
import { toErrorDetails } from '../Reporting/reportValues.js';
import { UsherContext, UsherContextValue } from './UsherContext.js';

/**
 * Props for {@link UsherProvider}.
 */
export interface UsherProviderProps {
    /** The version of the application, sent with every report. */
    applicationVersion?: string;

    /** The name of the application, shown with what a report sends. */
    applicationName?: string;

    /** Whether the report dialogs let support know who the reporter is unless they choose otherwise. Defaults to false. */
    shareIdentityByDefault?: boolean;

    /** The labels that replace the English defaults, for instance to localize them. */
    labels?: UsherLabelOverrides;

    /** The locale to format dates and times for; the browser's when not given. */
    locale?: string;

    /** How many recent correlation ids of Arc commands and queries a report carries. Defaults to 5. */
    correlationIdCount?: number;

    /** The HTTP header the backend reads the correlation id from. Defaults to `X-Correlation-ID`, as Arc does. */
    correlationIdHeader?: string;

    /** The application. */
    children?: ReactNode;
}

/**
 * Slots Usher into an application: renders the report and my issues dialogs once, gives {@link useUsher} and the
 * Usher buttons what they need, and tracks the correlation ids of the Arc commands and queries the application runs.
 * Render it inside `<Arc>`, the `DialogComponents` registration and a `CratisComponentsProvider` with a toaster.
 */
export const UsherProvider = ({
    applicationVersion = '',
    applicationName = '',
    shareIdentityByDefault = false,
    labels,
    locale,
    correlationIdCount = 5,
    correlationIdHeader = defaultCorrelationIdHeader,
    children
}: UsherProviderProps) => {
    const arc = useContext(ArcContext);
    const tracker = useRef<CorrelationTracker>(undefined);
    if (!tracker.current || tracker.current.capacity !== correlationIdCount) {
        tracker.current = new CorrelationTracker(correlationIdCount);
    }
    const correlationTracker = tracker.current;
    const lastLocation = useRef<SliceLocation>(SliceLocation.empty);

    const trackedArc = useMemo(() => ({
        ...arc,
        httpHeadersCallback: withCorrelationIdHeader(arc.httpHeadersCallback, correlationTracker, correlationIdHeader)
    }), [arc, correlationTracker, correlationIdHeader]);

    const [ReportIssueDialogWrapper, showReportIssueDialog] = useDialog<ReportSent, Parameters<typeof ReportIssueDialog>[0]>(ReportIssueDialog);
    const [ReportErrorDialogWrapper, showReportErrorDialog] = useDialog<ReportSent, Parameters<typeof ReportErrorDialog>[0]>(ReportErrorDialog);
    const [MyIssuesDialogWrapper, showMyIssuesDialog] = useDialog<object, Parameters<typeof MyIssuesDialog>[0]>(MyIssuesDialog);

    const resolvedLabels = useMemo(() => resolveLabels(labels), [labels]);

    const value = useMemo<UsherContextValue>(() => {
        const reportIssue = (location: SliceLocation) => {
            lastLocation.current = location;
            void showReportIssueDialog({ location, correlationIds: correlationTracker.correlationIds }).then(reportSent);
        };

        const showIssue = (issueId: Guid) => void showMyIssuesDialog({ issueId, onReportIssue: () => reportIssue(lastLocation.current) });

        const reportSent = ([result, response]: [DialogResult, ReportSent?]) => {
            if (result !== DialogResult.Ok || !response) {
                return;
            }

            toast.success({
                title: resolvedLabels.reportSent.title,
                action: { children: resolvedLabels.reportSent.view, onClick: () => showIssue(response.issueId) }
            });
        };

        return {
            isProvided: true,
            labels: resolvedLabels,
            applicationName,
            applicationVersion,
            shareIdentityByDefault,
            locale,
            untrackedArc: arc,
            recentCorrelationIds: () => correlationTracker.correlationIds,
            trackCorrelationId: correlationId => correlationTracker.track(correlationId),
            reportIssue,
            reportError: (error, location) => {
                lastLocation.current = location;
                void showReportErrorDialog({ error: toErrorDetails(error, location), location, correlationIds: correlationTracker.correlationIds }).then(reportSent);
            },
            showMyIssues: location => {
                lastLocation.current = location;
                void showMyIssuesDialog({ onReportIssue: () => reportIssue(location) });
            },
            showIssue
        };
    }, [resolvedLabels, applicationName, applicationVersion, shareIdentityByDefault, locale, arc, correlationTracker, showReportIssueDialog, showReportErrorDialog, showMyIssuesDialog]);

    return (
        <UsherContext.Provider value={value}>
            <ArcContext.Provider value={trackedArc}>
                {children}
            </ArcContext.Provider>
            <ReportIssueDialogWrapper />
            <ReportErrorDialogWrapper />
            <MyIssuesDialogWrapper />
        </UsherContext.Provider>
    );
};
