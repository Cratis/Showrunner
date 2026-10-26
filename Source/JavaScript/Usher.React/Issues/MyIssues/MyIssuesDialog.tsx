// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, Switch } from '@cratis/components/Common';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { useReducer } from 'react';
import { BugIcon } from '../../Icons.js';
import { UsherBoundary } from '../../Provider/UsherBoundary.js';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { IssueView } from '../IssueView/IssueView.js';
import { IssueList } from './IssueList.js';
import { navigate, startNavigation } from './MyIssuesNavigation.js';

/**
 * Props for {@link MyIssuesDialog}.
 */
export interface MyIssuesDialogProps {
    /** The issue to open on; the list of issues when not given. */
    issueId?: Guid;

    /** Opens the report a problem dialog; no report button when not given. */
    onReportIssue?: () => void;
}

/**
 * The my issues side sheet: the signed-in reporter's issues, and one issue at a time with its conversation. Open it
 * with `useDialog`.
 */
export const MyIssuesDialog = ({ issueId, onReportIssue }: MyIssuesDialogProps) => {
    const labels = useUsherLabels();
    const [navigation, dispatch] = useReducer(navigate, issueId, startNavigation);
    if (navigation.requestedIssueId !== issueId) {
        dispatch({ type: 'requested', issueId });
    }

    return (
        <Dialog
            title={labels.myIssues.title}
            subtitle={labels.myIssues.description}
            placement='end'
            width='720px'
            buttons={null}
            dismissable
            initialFocus={DialogInitialFocus.Content}
            closeAriaLabel={labels.common.close}>
            <UsherBoundary>
                {navigation.openIssueId
                    ? <IssueView issueId={navigation.openIssueId} onBack={() => dispatch({ type: 'back' })} />
                    : (
                        <div className='usher-my-issues'>
                            <div className='usher-my-issues__toolbar'>
                                {onReportIssue && (
                                    <Button variant='solid' tone='accent' size='small' icon={<BugIcon />} label={labels.myIssues.reportProblem} onClick={onReportIssue} />
                                )}
                                <Switch label={labels.myIssues.showClosed} checked={navigation.showClosed} onChange={showClosed => dispatch({ type: 'showClosedChanged', showClosed })} />
                            </div>
                            <IssueList showClosed={navigation.showClosed} onOpen={issue => dispatch({ type: 'opened', issueId: issue.id })} />
                        </div>
                    )}
            </UsherBoundary>
        </Dialog>
    );
};
