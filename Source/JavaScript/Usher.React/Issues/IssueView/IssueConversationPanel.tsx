// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ChatConversation } from '@cratis/components/Chat';
import { Message, Tag } from '@cratis/components/Display';
import { toastCommandResult } from '@cratis/components/Notifications';
import { AddDetails, AnswerQuestion, ConversationEntryKind, IssueConversation, IssueState } from '@cratis/showrunner.usher';
import { useMemo } from 'react';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { authorOf, ConversationMessage, conversationMessages, isOpenQuestion } from '../conversationMessages.js';

/**
 * Props for {@link IssueConversationPanel}.
 */
export interface IssueConversationPanelProps {
    /** The issue. */
    issue: IssueConversation;
}

/**
 * The conversation on an issue. A message the reporter sends answers support's open question, or adds details when
 * there is none. Once the reporter withdrew the issue, the composer is disabled: nothing more can be added to it.
 */
export const IssueConversationPanel = ({ issue }: IssueConversationPanelProps) => {
    const labels = useUsherLabels();
    const [addDetails] = AddDetails.use();
    const [answerQuestion] = AnswerQuestion.use();
    const messages = useMemo(() => conversationMessages(issue.id, issue.entries ?? [], labels), [issue.id, issue.entries, labels]);
    const openQuestionId = issue.openQuestion?.questionId;
    const isWithdrawn = issue.state === IssueState.withdrawn;

    const send = async (body: string) => {
        if (isWithdrawn) {
            return;
        }

        if (isOpenQuestion(openQuestionId)) {
            answerQuestion.issueId = issue.id;
            answerQuestion.questionId = openQuestionId;
            answerQuestion.answer = body;
            toastCommandResult(await answerQuestion.execute(), { showSuccess: false });
            return;
        }

        addDetails.issueId = issue.id;
        addDetails.details = body;
        toastCommandResult(await addDetails.execute(), { showSuccess: false });
    };

    const renderMessageExtra = (message: ConversationMessage) => {
        if (message.kind !== ConversationEntryKind.questionAsked) {
            return undefined;
        }

        const isOpen = isOpenQuestion(openQuestionId) && openQuestionId.equals(message.questionId);
        return (
            <span className='usher-conversation__question-state'>
                <Tag value={isOpen ? labels.issue.waitingForAnswer : labels.issue.answered} severity={isOpen ? 'warn' : 'success'} />
            </span>
        );
    };

    return (
        <section className='usher-conversation' data-closed={isWithdrawn || undefined} aria-label={issue.title}>
            <fieldset className='usher-conversation__fieldset' disabled={isWithdrawn}>
                <ChatConversation<ConversationMessage>
                    messages={messages}
                    onSendMessage={body => void send(body)}
                    authorOf={authorId => authorOf(authorId, labels)}
                    renderMessageExtra={renderMessageExtra}
                    quickReply={false}
                    labels={{
                        empty: labels.conversation.empty,
                        timestamps: {
                            justNow: labels.conversation.justNow,
                            minutesAgo: labels.conversation.minutesAgo,
                            hoursAgo: labels.conversation.hoursAgo
                        },
                        composer: {
                            placeholder: isWithdrawn ? labels.issue.withdrawnPlaceholder : labels.issue.composerPlaceholder,
                            send: labels.conversation.send,
                            sendMessage: labels.conversation.send
                        }
                    }} />
            </fieldset>
            {isWithdrawn && <Message severity='secondary' text={labels.issue.withdrawnNotice} live={false} />}
        </section>
    );
};
