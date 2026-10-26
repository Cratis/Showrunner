// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandDialog } from '@cratis/components/CommandDialog';
import { DropdownField } from '@cratis/components/CommandForm';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { AccessDuration, GrantAccess } from '@cratis/showrunner.usher';
import { useId, useState } from 'react';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { AccessNotice } from '../../Reporting/AccessNotice.js';
import { durationOptions } from '../../Reporting/durationOptions.js';

/**
 * Props for {@link GrantAccessDialog}.
 */
export interface GrantAccessDialogProps {
    /** The issue. */
    issueId: Guid;
}

/**
 * Lets the reporter grant support access to sign in as them on an issue for a while, stating until when.
 */
export const GrantAccessDialog = ({ issueId }: GrantAccessDialogProps) => {
    const labels = useUsherLabels();
    const noticeId = useId();
    const [duration, setDuration] = useState(AccessDuration.oneDay);

    return (
        <CommandDialog<GrantAccess>
            command={GrantAccess}
            title={labels.accessPanel.grantTitle}
            okLabel={labels.accessPanel.grantConfirm}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            initialFocus={DialogInitialFocus.Content}
            initialValues={{ issueId, accessDuration: AccessDuration.oneDay }}
            onFieldChange={(_command, fieldName, _oldValue, newValue) => {
                if (fieldName === 'accessDuration') {
                    setDuration(Number(newValue) as AccessDuration);
                }
            }}>
            <div className='usher-report-form'>
                <DropdownField<GrantAccess>
                    value={command => command.accessDuration}
                    title={labels.access.duration}
                    aria-describedby={noticeId}
                    options={durationOptions(labels)}
                    optionValue='value'
                    optionLabel='label' />
                <AccessNotice id={noticeId} duration={duration} />
            </div>
        </CommandDialog>
    );
};
