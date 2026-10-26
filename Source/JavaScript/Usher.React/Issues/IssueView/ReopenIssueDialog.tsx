// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandDialog } from '@cratis/components/CommandDialog';
import { TextAreaField } from '@cratis/components/CommandForm';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { ReopenIssue } from '@cratis/showrunner.usher';
import { useUsherLabels } from '../../Provider/UsherContext.js';

/**
 * Props for {@link ReopenIssueDialog}.
 */
export interface ReopenIssueDialogProps {
    /** The issue. */
    issueId: Guid;
}

/**
 * Lets the reporter reopen a resolved issue, saying why it is not fixed.
 */
export const ReopenIssueDialog = ({ issueId }: ReopenIssueDialogProps) => {
    const labels = useUsherLabels();

    return (
        <CommandDialog<ReopenIssue>
            command={ReopenIssue}
            title={labels.actions.reopenTitle}
            okLabel={labels.actions.reopen}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            initialFocus={DialogInitialFocus.Content}
            initialValues={{ issueId, reason: '' }}>
            <TextAreaField<ReopenIssue> value={command => command.reason} title={labels.actions.reopenReason} rows={4} />
        </CommandDialog>
    );
};
