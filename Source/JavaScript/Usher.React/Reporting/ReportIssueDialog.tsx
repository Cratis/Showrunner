// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult, useDialogContext } from '@cratis/arc.react/dialogs';
import { useSliceContext } from '@cratis/arc.react/slices';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { CommandDialog } from '@cratis/components/CommandDialog';
import { InputTextField, TextAreaField } from '@cratis/components/CommandForm';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { ReportIssue } from '@cratis/showrunner.usher';
import { useState } from 'react';
import { UsherBoundary } from '../Provider/UsherBoundary.js';
import { useUsherContext } from '../Provider/UsherContext.js';
import { AccessFields } from './AccessFields.js';
import { ReportSent } from './ReportSent.js';
import { reportIssueValues } from './reportValues.js';
import { SentWithReport } from './SentWithReport.js';
import { useAccessChoice } from './useAccessChoice.js';

/**
 * Props for {@link ReportIssueDialog}.
 */
export interface ReportIssueDialogProps {
    /** Where in the application the reporter is; the slice context the dialog is rendered in when not given. */
    location?: ArcSliceLocation;

    /** The correlation ids to send; the recent ones {@link UsherProvider} tracked when not given. */
    correlationIds?: Guid[];
}

/**
 * The report a problem dialog: the reporter describes a problem by hand, and chooses whether support may know who
 * they are and sign in as them. Open it with `useDialog`; it closes with {@link ReportSent} once the report is sent.
 */
export const ReportIssueDialog = ({ location, correlationIds }: ReportIssueDialogProps) => {
    const usher = useUsherContext();
    const sliceLocation = useSliceContext();
    const [values] = useState(() => reportIssueValues({
        issueId: Guid.create(),
        location: location ?? sliceLocation,
        correlationIds: correlationIds ?? usher.recentCorrelationIds(),
        applicationVersion: usher.applicationVersion,
        shareIdentity: usher.shareIdentityByDefault
    }));

    return (
        <UsherBoundary>
            <ReportIssueForm values={values} />
        </UsherBoundary>
    );
};

const ReportIssueForm = ({ values }: { values: ReturnType<typeof reportIssueValues> }) => {
    const { labels, shareIdentityByDefault } = useUsherContext();
    const { closeDialog } = useDialogContext<ReportIssueDialogProps, ReportSent>();
    const access = useAccessChoice(shareIdentityByDefault);

    return (
        <CommandDialog<ReportIssue>
            command={ReportIssue}
            title={labels.reportIssue.title}
            okLabel={labels.reportIssue.send}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            width='560px'
            initialFocus={DialogInitialFocus.Content}
            initialValues={access.initialValues(values)}
            currentValues={access.currentValues}
            onFieldChange={access.onFieldChange}
            onSuccess={() => closeDialog(DialogResult.Ok, { issueId: values.issueId! })}>
            <div className='usher-report-form'>
                <SentWithReport location={values.location!} correlationIdCount={values.correlationIds!.length} />
                <InputTextField<ReportIssue> value={command => command.summary} title={labels.reportIssue.summary} />
                <TextAreaField<ReportIssue> value={command => command.description} title={labels.reportIssue.description} rows={5} />
                <AccessFields<ReportIssue> choice={access.choice} />
            </div>
        </CommandDialog>
    );
};
