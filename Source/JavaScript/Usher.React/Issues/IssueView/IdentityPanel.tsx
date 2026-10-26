// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useDialog } from '@cratis/arc.react/dialogs';
import { Button } from '@cratis/components/Common';
import { IssueConversation, IssueState } from '@cratis/showrunner.usher';
import { useId } from 'react';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { ShareIdentityDialog } from './ShareIdentityDialog.js';

/**
 * Props for {@link IdentityPanel}.
 */
export interface IdentityPanelProps {
    /** The issue. */
    issue: IssueConversation;
}

/**
 * Shows whether support knows who the reporter is, and lets them share it when it does not.
 */
export const IdentityPanel = ({ issue }: IdentityPanelProps) => {
    const labels = useUsherLabels();
    const headingId = useId();
    const [ShareDialog, showShareDialog] = useDialog(ShareIdentityDialog);
    const canShare = !issue.identityShared && issue.state !== IssueState.withdrawn;

    return (
        <section className='usher-panel' aria-labelledby={headingId}>
            <h4 id={headingId} className='usher-panel__heading'>{labels.identity.heading}</h4>
            <p className='usher-panel__text'>{issue.identityShared ? labels.identity.known : labels.identity.unknown}</p>
            {canShare && (
                <Button variant='outline' size='small' label={labels.identity.share} onClick={() => void showShareDialog({ issueId: issue.id })} />
            )}
            <ShareDialog />
        </section>
    );
};
