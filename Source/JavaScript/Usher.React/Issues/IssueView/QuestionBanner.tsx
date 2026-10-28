// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { format } from '../../Labels/index.js';
import { useUsherLabels } from '../../Provider/UsherContext.js';

/**
 * Props for {@link QuestionBanner}.
 */
export interface QuestionBannerProps {
    /** What support asked. */
    question: string;
}

/**
 * Tells the reporter that support waits for an answer, and that their next message answers it.
 */
export const QuestionBanner = ({ question }: QuestionBannerProps) => {
    const labels = useUsherLabels();

    return (
        <div className='usher-question-banner'>
            <Message severity='warn' text={format(labels.issue.questionBanner, { question })} />
        </div>
    );
};
