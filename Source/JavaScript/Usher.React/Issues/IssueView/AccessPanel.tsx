// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons, DialogResult, useConfirmationDialog, useDialog } from '@cratis/arc.react/dialogs';
import { Button } from '@cratis/components/Common';
import { toastCommandResult } from '@cratis/components/Notifications';
import { IssueConversation, IssueState, RevokeAccess } from '@cratis/showrunner.usher';
import { useId, useState } from 'react';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { formatTime } from '../../Reporting/AccessChoice.js';
import { AccessStatus, accessStatusOf } from '../accessStatus.js';
import { GrantAccessDialog } from './GrantAccessDialog.js';
import { SupportSessions } from './SupportSessions.js';

/**
 * Props for {@link AccessPanel}.
 */
export interface AccessPanelProps {
    /** The issue. */
    issue: IssueConversation;
}

/**
 * Shows whether support may sign in as the reporter, lets them grant or revoke it, highlights support's pending
 * request, and lists the sessions in which support signed in.
 */
export const AccessPanel = ({ issue }: AccessPanelProps) => {
    const { labels, locale } = useUsherContext();
    const headingId = useId();
    const [GrantDialog, showGrantDialog] = useDialog(GrantAccessDialog);
    const [confirm] = useConfirmationDialog();
    const [revokeAccess] = RevokeAccess.use();
    const [requestDismissed, setRequestDismissed] = useState(false);
    const status = accessStatusOf(issue.access, new Date());
    const isWithdrawn = issue.state === IssueState.withdrawn;
    const showsRequest = status === AccessStatus.requested && !requestDismissed && !isWithdrawn;
    const validUntil = issue.access?.validUntil instanceof Date ? issue.access.validUntil : new Date(issue.access?.validUntil);

    const heading = status === AccessStatus.granted
        ? labels.accessPanel.grantedHeading
        : showsRequest ? labels.accessPanel.requestedHeading : labels.accessPanel.notGrantedHeading;

    const revoke = async () => {
        const answer = await confirm(labels.accessPanel.revokeTitle, labels.accessPanel.revokeMessage, DialogButtons.YesNo);
        if (answer !== DialogResult.Yes) {
            return;
        }

        revokeAccess.issueId = issue.id;
        toastCommandResult(await revokeAccess.execute(), { showSuccess: false });
    };

    const grant = () => void showGrantDialog({ issueId: issue.id });

    return (
        <section className='usher-panel' data-highlighted={showsRequest || undefined} aria-labelledby={headingId}>
            <h4 id={headingId} className='usher-panel__heading'>{heading}</h4>
            {status === AccessStatus.granted && (
                <>
                    <p className='usher-panel__text'>{format(labels.accessPanel.grantedUntil, { time: formatTime(validUntil, locale) })}</p>
                    <Button variant='outline' tone='critical' size='small' label={labels.accessPanel.revoke} onClick={() => void revoke()} />
                </>
            )}
            {showsRequest && (
                <>
                    <p className='usher-panel__text'>{format(labels.accessPanel.requestReason, { reason: issue.access.request.reason })}</p>
                    <div className='usher-panel__actions'>
                        <Button variant='solid' tone='accent' size='small' label={labels.accessPanel.grant} onClick={grant} />
                        <Button variant='ghost' size='small' label={labels.accessPanel.notNow} onClick={() => setRequestDismissed(true)} />
                    </div>
                </>
            )}
            {status !== AccessStatus.granted && !showsRequest && (
                <>
                    <p className='usher-panel__text'>{labels.accessPanel.notGranted}</p>
                    {!isWithdrawn && <Button variant='outline' size='small' label={labels.accessPanel.grant} onClick={grant} />}
                </>
            )}
            <SupportSessions sessions={issue.access?.sessions ?? []} />
            <GrantDialog />
        </section>
    );
};
