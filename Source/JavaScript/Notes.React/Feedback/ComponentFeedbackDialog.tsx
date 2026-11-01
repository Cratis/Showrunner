// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult, useDialogContext } from '@cratis/arc.react/dialogs';
import { CommandDialog } from '@cratis/components/CommandDialog';
import { ComponentHighlight, PickedComponent as PickedByUser, PresentationChange, PresentationPropertyEditor } from '@cratis/components/ComponentPicker';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { Guid } from '@cratis/fundamentals';
import { GiveNoteOnComponent, NoteContext, SuggestComponentPropertyChange } from '@cratis/showrunner.notes';
import { useMemo, useRef, useState } from 'react';
import { describeContext, toNotePickedComponent } from '../Context/index.js';
import { format } from '../Labels/index.js';
import { useNotesContext } from '../Provider/NotesContext.js';
import { ComponentFeedbackMode, ComponentModeSwitch } from './ComponentModeSwitch.js';
import { ContextSummary } from './ContextSummary.js';
import { FeedbackGiven } from './FeedbackDialog.js';
import { defaultKind } from './kindOptions.js';
import { KindAndText } from './KindAndText.js';
import { propertySuggestions } from './propertySuggestions.js';

/**
 * Props for {@link ComponentFeedbackDialog}.
 */
export interface ComponentFeedbackDialogProps {
    /** What the user was looking at when they chose to point at something. */
    context: NoteContext;

    /** The component the user picked. */
    picked: PickedByUser;

    /** The mode to open in. Defaults to describing the component. */
    initialMode?: ComponentFeedbackMode;
}

/**
 * The give feedback on a component dialog. The picked component stays highlighted while it is open. The user either
 * describes it (`GiveNoteOnComponent`) or changes it: the property editor previews each change live and reverts it when
 * the dialog closes, and one `SuggestComponentPropertyChange` is sent per changed property. Open it with `useDialog`.
 */
export const ComponentFeedbackDialog = ({ context, picked, initialMode = 'describe' }: ComponentFeedbackDialogProps) => {
    const { labels } = useNotesContext();
    const [noteId] = useState(() => Guid.create());
    const [mode, setMode] = useState<ComponentFeedbackMode>(initialMode);
    const component = useMemo(() => toNotePickedComponent(picked, context.location), [picked, context]);
    const switchMode = <ComponentModeSwitch mode={mode} onChange={setMode} />;
    const summary = <ContextSummary text={format(labels.component.about, { component: picked.id ? `${picked.component} (${picked.id})` : picked.component })}>
        <span className='notes-context-summary__text'>{describeContext(context, labels)}</span>
    </ContextSummary>;

    return (
        <>
            <ComponentHighlight target={picked} />
            {mode === 'describe'
                ? <DescribeComponent context={context} component={component} noteId={noteId} header={<>{switchMode}{summary}</>} />
                : <ChangeComponent context={context} picked={picked} component={component} noteId={noteId} header={<>{switchMode}{summary}</>} />}
        </>
    );
};

interface ModeProps {
    context: NoteContext;
    component: ReturnType<typeof toNotePickedComponent>;
    noteId: Guid;
    header: React.ReactNode;
}

const DescribeComponent = ({ context, component, noteId, header }: ModeProps) => {
    const { labels } = useNotesContext();
    const { closeDialog } = useDialogContext<ComponentFeedbackDialogProps, FeedbackGiven>();

    return (
        <CommandDialog<GiveNoteOnComponent>
            command={GiveNoteOnComponent}
            title={labels.component.title}
            okLabel={labels.feedback.send}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            width='600px'
            initialFocus={DialogInitialFocus.Content}
            initialValues={{ context, component, noteId, kind: defaultKind }}
            onSuccess={() => closeDialog(DialogResult.Ok, { noteId })}>
            <div className='notes-feedback-form'>
                {header}
                <KindAndText<GiveNoteOnComponent> />
            </div>
        </CommandDialog>
    );
};

const ChangeComponent = ({ context, picked, component, noteId, header }: ModeProps & { picked: PickedByUser }) => {
    const { labels } = useNotesContext();
    const [changes, setChanges] = useState<PresentationChange[]>([]);
    const [isSending, setIsSending] = useState(false);
    const [failed, setFailed] = useState(0);
    const sending = useRef(false);
    const [suggestion, setSuggestion] = SuggestComponentPropertyChange.use();

    const send = async (): Promise<boolean> => {
        if (sending.current || changes.length === 0) {
            return false;
        }

        sending.current = true;
        setIsSending(true);
        setFailed(0);
        let failures = 0;
        try {
            for (const values of propertySuggestions(changes, context, component, noteId)) {
                setSuggestion(values);
                const result = await suggestion.execute();
                if (!result.isSuccess) {
                    failures++;
                }
            }
        } finally {
            sending.current = false;
            setIsSending(false);
        }

        setFailed(failures);
        return failures === 0;
    };

    return (
        <Dialog
            title={labels.component.title}
            okLabel={labels.component.sendChanges}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            width='600px'
            initialFocus={DialogInitialFocus.Content}
            isValid={changes.length > 0}
            isBusy={isSending}
            onConfirm={send}>
            <div className='notes-feedback-form'>
                {header}
                {picked.presentationProperties.length === 0
                    ? <p className='notes-change-notice'>{labels.component.nothingToChange}</p>
                    : (
                        <>
                            <p className='notes-change-notice'>{labels.component.previewNotice}</p>
                            <PresentationPropertyEditor picked={picked} preview onChange={setChanges} />
                            {changes.length === 0 && <p className='notes-change-notice' role='status'>{labels.component.noChanges}</p>}
                        </>
                    )}
                {failed > 0 && <p className='notes-change-failure' role='alert'>{format(labels.component.failed, { count: failed })}</p>}
            </div>
        </Dialog>
    );
};
