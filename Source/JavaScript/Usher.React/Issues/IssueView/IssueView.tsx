// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { Guid } from '@cratis/fundamentals';
import { ForIssue } from '@cratis/showrunner.usher';
import { UsherBoundary } from '../../Provider/UsherBoundary.js';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { isOpenQuestion } from '../conversationMessages.js';
import { AccessPanel } from './AccessPanel.js';
import { IdentityPanel } from './IdentityPanel.js';
import { IssueActions } from './IssueActions.js';
import { IssueConversationPanel } from './IssueConversationPanel.js';
import { IssueHeader } from './IssueHeader.js';
import { QuestionBanner } from './QuestionBanner.js';
import { StatusStrip } from './StatusStrip.js';

/**
 * Props for {@link IssueView}.
 */
export interface IssueViewProps {
    /** The issue to show. */
    issueId: Guid;

    /** Goes back to the list of issues; no back button when not given. */
    onBack?: () => void;
}

/**
 * One of the signed-in reporter's issues: its state, the conversation with support, who support knows the reporter
 * is, support's access, and what the reporter can do with it. It stays current as support works on the issue.
 * Usable on its own, such as on a page of the solution, or inside the my issues side sheet.
 */
export const IssueView = (props: IssueViewProps) => (
    <UsherBoundary>
        <IssueViewContent {...props} />
    </UsherBoundary>
);

const IssueViewContent = ({ issueId, onBack }: IssueViewProps) => {
    const labels = useUsherLabels();
    const [result] = ForIssue.use({ issueId });
    const issue = result.data;
    const isLoaded = result.hasData && issue?.id !== undefined;

    if (!isLoaded) {
        return (
            <div className='usher-issue-view'>
                <Message severity={result.isPerforming ? 'info' : 'secondary'} text={result.isPerforming ? labels.issue.loading : labels.issue.notFound} />
            </div>
        );
    }

    const openQuestion = issue.openQuestion;

    return (
        <div className='usher-issue-view'>
            <IssueHeader issue={issue} onBack={onBack} />
            <StatusStrip state={issue.state} />
            {isOpenQuestion(openQuestion?.questionId) && <QuestionBanner question={openQuestion.question} />}
            <div className='usher-issue-view__body'>
                <IssueConversationPanel issue={issue} />
                <aside className='usher-issue-view__side'>
                    <IdentityPanel issue={issue} />
                    <AccessPanel issue={issue} />
                    <IssueActions issue={issue} />
                </aside>
            </div>
        </div>
    );
};
