// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons, DialogResult, useConfirmationDialog, useDialog } from '@cratis/arc.react/dialogs';
import { Button } from '@cratis/components/Common';
import { toastCommandResult } from '@cratis/components/Notifications';
import { IssueConversation, IssueState, WithdrawIssue } from '@cratis/showrunner.usher';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { isClosed } from '../issueStates.js';
import { ReopenIssueDialog } from './ReopenIssueDialog.js';

/**
 * Props for {@link IssueActions}.
 */
export interface IssueActionsProps {
    /** The issue. */
    issue: IssueConversation;
}

/**
 * What the reporter can do with an issue: reopen it once resolved, and withdraw it while it is open.
 */
export const IssueActions = ({ issue }: IssueActionsProps) => {
    const labels = useUsherLabels();
    const [ReopenDialog, showReopenDialog] = useDialog(ReopenIssueDialog);
    const [confirm] = useConfirmationDialog();
    const [withdrawIssue] = WithdrawIssue.use();
    const canReopen = issue.state === IssueState.resolved;
    const canWithdraw = !isClosed(issue.state);

    const withdraw = async () => {
        const answer = await confirm(labels.actions.withdrawTitle, labels.actions.withdrawMessage, DialogButtons.YesNo);
        if (answer !== DialogResult.Yes) {
            return;
        }

        withdrawIssue.issueId = issue.id;
        toastCommandResult(await withdrawIssue.execute(), { showSuccess: false });
    };

    if (!canReopen && !canWithdraw) {
        return null;
    }

    return (
        <div className='usher-issue-actions'>
            {canReopen && <Button variant='outline' size='small' label={labels.actions.reopen} onClick={() => void showReopenDialog({ issueId: issue.id })} />}
            {canWithdraw && <Button variant='ghost' tone='critical' size='small' label={labels.actions.withdraw} onClick={() => void withdraw()} />}
            <ReopenDialog />
        </div>
    );
};
