// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '@cratis/components/Common';
import { Tag } from '@cratis/components/Display';
import { IssueConversation } from '@cratis/showrunner.usher';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { formatTime } from '../../Reporting/AccessChoice.js';
import { formatLocation } from '../../Reporting/formatLocation.js';
import { BackIcon } from '../../Icons.js';
import { stateBadgeFor } from '../issueStates.js';
import { isFixReleased } from '../MyIssuesList.js';
import { isWebAddress } from '../webAddress.js';

/**
 * Props for {@link IssueHeader}.
 */
export interface IssueHeaderProps {
    /** The issue. */
    issue: IssueConversation;

    /** Goes back to the list of issues; no back button when not given. */
    onBack?: () => void;
}

/**
 * The header of an issue: its title and state, when and where it was reported, and what it is linked to.
 */
export const IssueHeader = ({ issue, onBack }: IssueHeaderProps) => {
    const { labels, locale } = useUsherContext();
    const badge = stateBadgeFor(issue.state, labels);
    const reportedAt = issue.reportedAt instanceof Date ? issue.reportedAt : new Date(issue.reportedAt);
    const location = formatLocation(issue.location, labels.common.locationSeparator, labels.common.unknownLocation);
    const workItem = issue.workItem;
    const hasWorkItem = (workItem?.reference ?? '').length > 0;

    return (
        <header className='usher-issue-header'>
            {onBack && (
                <Button variant='ghost' size='small' icon={<BackIcon />} onClick={onBack} label={labels.issue.back} />
            )}
            <div className='usher-issue-header__title-row'>
                <h3 className='usher-issue-header__title'>{issue.title}</h3>
                <Tag value={badge.label} severity={badge.severity} />
            </div>
            <p className='usher-issue-header__meta'>
                {format(labels.issue.reported, { date: formatTime(reportedAt, locale) })} · {location}
            </p>
            {(issue.knownIssueTitle?.length > 0 || hasWorkItem) && (
                <p className='usher-issue-header__links'>
                    {issue.knownIssueTitle?.length > 0 && (
                        <span>{labels.issue.knownIssue} <strong>{issue.knownIssueTitle}</strong></span>
                    )}
                    {hasWorkItem && (
                        <span>
                            {labels.issue.trackedIn}{' '}
                            {isWebAddress(workItem.url)
                                ? <a href={workItem.url} target='_blank' rel='noreferrer'>{workItem.reference}</a>
                                : <strong>{workItem.reference}</strong>}
                            {' · '}
                            {isFixReleased(issue) ? format(labels.issue.fixedIn, { version: issue.fixedInVersion }) : labels.issue.notReleased}
                        </span>
                    )}
                </p>
            )}
        </header>
    );
};
