// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ApplicationContext, useApplicationContextStore } from '@cratis/arc.react';
import { DialogResult, useDialog } from '@cratis/arc.react/dialogs';
import { useIdentity } from '@cratis/arc.react/identity';
import { useComponentPicker } from '@cratis/components/ComponentPicker';
import { DialogHeaderContext } from '@cratis/components/Dialogs';
import { toast } from '@cratis/components/Notifications';
import { Guid } from '@cratis/fundamentals';
import { Availability } from '@cratis/showrunner.notes';
import { ReactNode, useCallback, useMemo, useRef } from 'react';
import { buildNoteContext, DialogReference, resolveComponentContext } from '../Context/index.js';
import { LocationParts } from '../Context/formatLocation.js';
import { ComponentFeedbackDialog } from '../Feedback/ComponentFeedbackDialog.js';
import { FeedbackDialog, FeedbackGiven } from '../Feedback/FeedbackDialog.js';
import { SlicesFeedbackDialog } from '../Feedback/SlicesFeedbackDialog.js';
import { NotesLabelOverrides, resolveLabels } from '../Labels/index.js';
import { MyFeedbackDialog } from '../MyFeedback/MyFeedbackDialog.js';
import { NotesContext, NotesContextValue } from './NotesContext.js';
import { isNotesAvailable } from './notesVisibility.js';

/**
 * Props for {@link NotesProvider}.
 */
export interface NotesProviderProps {
    /** Whether the feedback button registers itself in the header of dialogs that have a slice. Defaults to false. It takes effect through `notesDialogHeaderActions` on `CratisComponentsProvider`. */
    enabled?: boolean;

    /** The labels that replace the English defaults, for instance to localize them. */
    labels?: NotesLabelOverrides;

    /** Every slice the host knows about, for the search of the give feedback on slices dialog. */
    knownSlices?: readonly LocationParts[];

    /** The application. */
    children?: ReactNode;
}

const emptySnapshot = new ApplicationContext().getSnapshot();

const dialogReferenceOf = (dialog?: DialogHeaderContext): DialogReference | undefined =>
    dialog?.sliceLocation ? { title: dialog.title, location: dialog.sliceLocation } : undefined;

/**
 * Slots Notes into an application: renders the feedback dialogs and the component picker once, asks whether Notes is
 * switched on, and gives the Notes buttons and `useNotes` what they need. Render it inside `<Arc>`, the `DialogComponents`
 * registration, an `IdentityProvider` and a `CratisComponentsProvider` with a toaster. Turn on `<Arc sliceMarkers>` for
 * Notes to know the slice of a component the user points at.
 */
export const NotesProvider = ({ enabled = false, labels, knownSlices = [], children }: NotesProviderProps) => {
    const store = useApplicationContextStore();
    const identity = useIdentity();
    const [availability] = Availability.use();
    const isAvailable = isNotesAvailable(identity.isSet, availability.data?.isEnabled);
    const resolvedLabels = useMemo(() => resolveLabels(labels), [labels]);
    const busy = useRef(false);

    const [FeedbackDialogWrapper, showFeedbackDialog] = useDialog<FeedbackGiven, Parameters<typeof FeedbackDialog>[0]>(FeedbackDialog);
    const [SlicesDialogWrapper, showSlicesDialog] = useDialog<FeedbackGiven, Parameters<typeof SlicesFeedbackDialog>[0]>(SlicesFeedbackDialog);
    const [ComponentDialogWrapper, showComponentDialog] = useDialog<FeedbackGiven, Parameters<typeof ComponentFeedbackDialog>[0]>(ComponentFeedbackDialog);
    const [MyFeedbackDialogWrapper, showMyFeedbackDialog] = useDialog<object, object>(MyFeedbackDialog);
    const picker = useComponentPicker({ resolve: resolveComponentContext });

    const capture = useCallback((dialog?: DialogHeaderContext) => buildNoteContext({
        snapshot: store?.getSnapshot() ?? emptySnapshot,
        route: window.location.pathname,
        correlationId: Guid.create(),
        dialog: dialogReferenceOf(dialog)
    }), [store]);

    const whileOpen = useCallback(async <TResult,>(open: () => Promise<[DialogResult, TResult?]>, announceSent = true) => {
        if (busy.current) {
            return;
        }

        busy.current = true;
        try {
            const [result] = await open();
            if (announceSent && result === DialogResult.Ok) {
                toast.success({ title: resolvedLabels.feedback.sent });
            }
        } finally {
            busy.current = false;
        }
    }, [resolvedLabels]);

    const value = useMemo<NotesContextValue>(() => ({
        isProvided: true,
        labels: resolvedLabels,
        isAvailable,
        showsInDialogHeaders: enabled,
        isPicking: picker.isPicking,
        giveFeedback: dialog => void whileOpen(() => showFeedbackDialog({ context: capture(dialog) })),
        giveFeedbackOnScreen: dialog => void whileOpen(() => showFeedbackDialog({ context: capture(dialog), about: 'screen' })),
        giveFeedbackOnSlices: dialog => void whileOpen(() => showSlicesDialog({ context: capture(dialog), knownSlices })),
        pointAtSomething: dialog => {
            if (busy.current || picker.isPicking) {
                return;
            }

            const context = capture(dialog);
            void picker.start().then(picked => {
                if (picked) {
                    void whileOpen(() => showComponentDialog({ context, picked }));
                }
            });
        },
        showMyFeedback: () => void whileOpen(() => showMyFeedbackDialog({}), false)
    }), [resolvedLabels, isAvailable, enabled, picker, knownSlices, capture, whileOpen, showFeedbackDialog, showSlicesDialog, showComponentDialog, showMyFeedbackDialog]);

    return (
        <NotesContext.Provider value={value}>
            {children}
            <FeedbackDialogWrapper />
            <SlicesDialogWrapper />
            <ComponentDialogWrapper />
            <MyFeedbackDialogWrapper />
            {picker.picker}
        </NotesContext.Provider>
    );
};
