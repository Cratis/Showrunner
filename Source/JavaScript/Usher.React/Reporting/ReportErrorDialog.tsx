// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult, useDialogContext } from '@cratis/arc.react/dialogs';
import { useSliceContext } from '@cratis/arc.react/slices';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { CommandDialog } from '@cratis/components/CommandDialog';
import { TextAreaField } from '@cratis/components/CommandForm';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { IReportError, ReportError } from '@cratis/showrunner.usher';
import { useState } from 'react';
import { UsherBoundary } from '../Provider/UsherBoundary.js';
import { useUsherContext } from '../Provider/UsherContext.js';
import { AccessFields } from './AccessFields.js';
import { ReportSent } from './ReportSent.js';
import { reportErrorValues, toErrorDetails } from './reportValues.js';
import { SentWithReport } from './SentWithReport.js';
import { TechnicalDetails } from './TechnicalDetails.js';
import { useAccessChoice } from './useAccessChoice.js';

/**
 * Props for {@link ReportErrorDialog}.
 */
export interface ReportErrorDialogProps {
    /** The error: `ErrorDetails` from `@cratis/showrunner.usher`, or anything thrown. */
    error?: unknown;

    /** Where in the application the reporter is, when the error does not say; the slice context the dialog is rendered in when not given. */
    location?: ArcSliceLocation;

    /** The correlation ids to send besides the error's own; the recent ones {@link UsherProvider} tracked when not given. */
    correlationIds?: Guid[];
}

/**
 * The report this error dialog: the error is captured already, and the reporter may say what they were doing and
 * choose whether support may know who they are and sign in as them. The technical details are collapsed. Open it
 * with `useDialog`; it closes with {@link ReportSent} once the report is sent.
 */
export const ReportErrorDialog = ({ error, location, correlationIds }: ReportErrorDialogProps) => {
    const usher = useUsherContext();
    const sliceLocation = useSliceContext();
    const [values] = useState(() => reportErrorValues(toErrorDetails(error, location ?? sliceLocation), {
        issueId: Guid.create(),
        location: location ?? sliceLocation,
        correlationIds: correlationIds ?? usher.recentCorrelationIds(),
        applicationVersion: usher.applicationVersion,
        shareIdentity: usher.shareIdentityByDefault
    }));

    return (
        <UsherBoundary>
            <ReportErrorForm values={values} />
        </UsherBoundary>
    );
};

const ReportErrorForm = ({ values }: { values: IReportError }) => {
    const { labels, shareIdentityByDefault } = useUsherContext();
    const { closeDialog } = useDialogContext<ReportErrorDialogProps, ReportSent>();
    const access = useAccessChoice(shareIdentityByDefault);

    return (
        <CommandDialog<ReportError>
            command={ReportError}
            title={labels.reportError.title}
            subtitle={values.errorMessage}
            okLabel={labels.reportError.send}
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
                <TechnicalDetails
                    errorType={values.errorType!}
                    correlationIds={values.correlationIds!}
                    stackFrames={values.stackFrames!} />
                <TextAreaField<ReportError>
                    value={command => command.details}
                    title={labels.reportError.details}
                    placeholder={labels.reportError.detailsPlaceholder}
                    required={false}
                    rows={3} />
                <AccessFields<ReportError> choice={access.choice} />
            </div>
        </CommandDialog>
    );
};
