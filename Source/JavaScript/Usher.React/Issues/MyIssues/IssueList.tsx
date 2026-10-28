// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Column, DataTableFilterMatchMode, DataTableForObservableQuery } from '@cratis/components/DataTables';
import { Tag } from '@cratis/components/Display';
import { IssueKind, MyIssues, ReportedIssue } from '@cratis/showrunner.usher';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { formatTime } from '../../Reporting/AccessChoice.js';
import { isClosed, stateBadgeFor, statusSteps } from '../issueStates.js';
import { isFixReleased } from '../MyIssuesList.js';

/**
 * Props for {@link IssueList}.
 */
export interface IssueListProps {
    /** Whether to list closed issues too. */
    showClosed: boolean;

    /** Opens an issue. */
    onOpen: (issue: ReportedIssue) => void;
}

const openStates = statusSteps.filter(state => !isClosed(state));

/**
 * The signed-in reporter's issues, most recent activity first, as they change.
 */
export const IssueList = ({ showClosed, onOpen }: IssueListProps) => {
    const { labels, locale } = useUsherContext();

    return (
        <DataTableForObservableQuery<MyIssues, ReportedIssue, object>
            key={showClosed ? 'all' : 'open'}
            query={MyIssues}
            dataKey='id'
            emptyMessage={labels.myIssues.empty}
            filteredEmptyMessage={labels.myIssues.noOpenIssues}
            loadingMessage={labels.myIssues.loading}
            failureMessage={labels.myIssues.failed}
            unauthorizedMessage={labels.myIssues.unauthorized}
            defaultFilters={showClosed ? {} : { state: { value: openStates, matchMode: DataTableFilterMatchMode.In } }}
            selectionMode='single'
            selection={null}
            onSelectionChange={event => {
                if (event.value) {
                    onOpen(event.value);
                }
            }}>
            <Column<ReportedIssue>
                field='title'
                header={labels.myIssues.issueColumn}
                body={issue => (
                    <span className='usher-issue-list__title'>
                        <span>{issue.title}</span>
                        <span className='usher-issue-list__kind'>{issue.kind === IssueKind.error ? labels.myIssues.errorKind : labels.myIssues.manualKind}</span>
                    </span>
                )} />
            <Column<ReportedIssue>
                field='state'
                header={labels.myIssues.stateColumn}
                body={issue => {
                    const badge = stateBadgeFor(issue.state, labels);
                    return (
                        <span className='usher-issue-list__state'>
                            <Tag value={badge.label} severity={badge.severity} />
                            {issue.waitingOnReporter && <Tag value={labels.myIssues.waitingForYou} severity='warn' />}
                            {isFixReleased(issue) && <Tag value={format(labels.myIssues.fixedIn, { version: issue.fixedInVersion })} severity='success' />}
                        </span>
                    );
                }} />
            <Column<ReportedIssue>
                field='lastActivityAt'
                header={labels.myIssues.lastActivityColumn}
                body={issue => formatTime(issue.lastActivityAt instanceof Date ? issue.lastActivityAt : new Date(issue.lastActivityAt), locale)} />
        </DataTableForObservableQuery>
    );
};
