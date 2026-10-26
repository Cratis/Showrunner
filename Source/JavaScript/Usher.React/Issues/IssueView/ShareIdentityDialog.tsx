// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandDialog } from '@cratis/components/CommandDialog';
import { DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { ShareIdentity } from '@cratis/showrunner.usher';
import { useUsherLabels } from '../../Provider/UsherContext.js';

/**
 * Props for {@link ShareIdentityDialog}.
 */
export interface ShareIdentityDialogProps {
    /** The issue. */
    issueId: Guid;
}

/**
 * Asks the reporter to confirm that support may know who they are on an issue. Sharing cannot be undone, so the
 * dialog opens with focus on cancel.
 */
export const ShareIdentityDialog = ({ issueId }: ShareIdentityDialogProps) => {
    const labels = useUsherLabels();

    return (
        <CommandDialog<ShareIdentity>
            command={ShareIdentity}
            title={labels.identity.shareTitle}
            okLabel={labels.identity.shareConfirm}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            initialFocus={DialogInitialFocus.Cancel}
            initialValues={{ issueId }}>
            <p className='usher-dialog-text'>{labels.identity.shareDescription}</p>
        </CommandDialog>
    );
};
