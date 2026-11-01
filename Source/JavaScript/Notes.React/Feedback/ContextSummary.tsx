// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReactNode } from 'react';
import { useNotesLabels } from '../Provider/NotesContext.js';

/**
 * Props for {@link ContextSummary}.
 */
export interface ContextSummaryProps {
    /** What the note is about, in plain language. */
    text: string;

    /** More to show under it. */
    children?: ReactNode;
}

/**
 * Tells the user in plain language what their note is about, before they write it.
 */
export const ContextSummary = ({ text, children }: ContextSummaryProps) => {
    const labels = useNotesLabels();

    return (
        <div className='notes-context-summary' role='group' aria-label={labels.common.aboutHeading}>
            <span className='notes-context-summary__heading'>{labels.common.aboutHeading}</span>
            <span className='notes-context-summary__text'>{text}</span>
            {children}
        </div>
    );
};
