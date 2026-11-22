// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons } from '@cratis/arc.react/dialogs';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { useMemo } from 'react';
import { resolveLabels } from '../Labels/index.js';
import { Playbill, PlaybillProps } from '../Playbill.js';

/**
 * Props for {@link PlaybillHistoryDialog}.
 */
export interface PlaybillHistoryDialogProps extends Omit<PlaybillProps, 'selected' | 'onSelect'> {
    /** Whether the dialog is shown. */
    open: boolean;
    /** Called when the person closes the dialog. */
    onClose: () => void;
}

/**
 * The whole release history in a dialog, for an application that has no page for it: the "What's new" entry of a menu or the
 * "About" dialog opens it on request. It is the {@link Playbill} page inside a dialog, so showing a release that has not been
 * seen records it as seen. Mount it once in the application shell, inside Arc's context, and control it with `open`.
 */
export const PlaybillHistoryDialog = ({ open, onClose, labels, ...page }: PlaybillHistoryDialogProps) => {
    const resolved = useMemo(() => resolveLabels(labels), [labels]);

    if (!open) {
        return <></>;
    }

    return (
        <Dialog
            title={resolved.title}
            width='56rem'
            buttons={DialogButtons.Ok}
            okLabel={resolved.acknowledge}
            closeAriaLabel={resolved.close}
            initialFocus={DialogInitialFocus.Content}
            onConfirm={onClose}
            onCancel={onClose}>
            <div className='playbill-history'>
                <Playbill labels={labels} {...page} />
            </div>
        </Dialog>
    );
};
