// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { RadioGroupField, TextAreaField } from '@cratis/components/CommandForm';
import { NoteKind } from '@cratis/showrunner.notes';
import { useNotesLabels } from '../Provider/NotesContext.js';
import { kindOptions } from './kindOptions.js';

/**
 * The command values {@link KindAndText} binds to.
 */
export interface KindAndTextValues {
    /** What kind of feedback it is. */
    kind: NoteKind;

    /** What the user writes. */
    text: string;
}

/**
 * The kind selector and the text field every feedback dialog has.
 */
export const KindAndText = <TCommand extends KindAndTextValues>() => {
    const labels = useNotesLabels();

    return (
        <div className='notes-kind-and-text'>
            <RadioGroupField<TCommand>
                value={command => command.kind}
                title={labels.kinds.title}
                options={kindOptions(labels)}
                optionValue='value'
                optionLabel='label'
                layout='horizontal' />
            <TextAreaField<TCommand> value={command => command.text} title={labels.feedback.text} rows={5} />
        </div>
    );
};
