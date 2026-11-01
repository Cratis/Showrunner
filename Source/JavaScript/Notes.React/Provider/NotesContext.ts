// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogHeaderContext } from '@cratis/components/Dialogs';
import { createContext, useContext } from 'react';
import { LocationParts } from '../Context/formatLocation.js';
import { NotesLabels, resolveLabels } from '../Labels/index.js';

/**
 * What {@link NotesProvider} gives the Notes components.
 */
export interface NotesContextValue {
    /** Whether a {@link NotesProvider} provides the value; the defaults otherwise. */
    readonly isProvided: boolean;

    /** The labels to show. */
    readonly labels: NotesLabels;

    /** Whether the Notes components render: the user is signed in and Notes is switched on. */
    readonly isAvailable: boolean;

    /** Whether the feedback button registers itself in the header of dialogs that have a slice. */
    readonly showsInDialogHeaders: boolean;

    /** Whether picking a component is going on. */
    readonly isPicking: boolean;

    /**
     * Opens the dialog that gives feedback on what the user is looking at.
     * @param dialog The dialog the button is in; the feedback is about the dialog when given.
     */
    giveFeedback(dialog?: DialogHeaderContext): void;

    /**
     * Opens the dialog that gives feedback on specific slices.
     * @param dialog The dialog the button is in.
     */
    giveFeedbackOnSlices(dialog?: DialogHeaderContext): void;

    /**
     * Opens the dialog that gives feedback on the screen.
     * @param dialog The dialog the button is in.
     */
    giveFeedbackOnScreen(dialog?: DialogHeaderContext): void;

    /**
     * Starts pointing at a component, and opens the dialog that gives feedback on it once one is picked.
     * @param dialog The dialog the button is in.
     */
    pointAtSomething(dialog?: DialogHeaderContext): void;

    /**
     * Opens the user's own feedback.
     */
    showMyFeedback(): void;
}

const noop = () => {
    /* Without a NotesProvider there are no dialogs to open. */
};

/**
 * The value without a {@link NotesProvider}: English labels, nothing available and nothing to open.
 */
export const defaultNotesContextValue: NotesContextValue = {
    isProvided: false,
    labels: resolveLabels(),
    isAvailable: false,
    showsInDialogHeaders: false,
    isPicking: false,
    giveFeedback: noop,
    giveFeedbackOnSlices: noop,
    giveFeedbackOnScreen: noop,
    pointAtSomething: noop,
    showMyFeedback: noop
};

/**
 * The React context {@link NotesProvider} provides.
 */
export const NotesContext = createContext<NotesContextValue>(defaultNotesContextValue);

/**
 * Gets what {@link NotesProvider} provides, or the defaults outside one.
 * @returns The {@link NotesContextValue}.
 */
export const useNotesContext = (): NotesContextValue => useContext(NotesContext);

/**
 * Gets the labels to show.
 * @returns The {@link NotesLabels}.
 */
export const useNotesLabels = (): NotesLabels => useContext(NotesContext).labels;

/**
 * What a component that opens Notes dialogs from its own code can do.
 */
export type Notes = Pick<NotesContextValue, 'labels' | 'isAvailable' | 'giveFeedback' | 'giveFeedbackOnSlices' | 'giveFeedbackOnScreen' | 'pointAtSomething' | 'showMyFeedback'>;

/**
 * Uses Notes from any component inside a {@link NotesProvider}, to open its dialogs from a menu or shortcut of your own.
 * @returns The {@link Notes}.
 */
export const useNotes = (): Notes => useContext(NotesContext);

/**
 * The slice location parts of a dialog header context, when it has one.
 * @param dialog The dialog the button is in.
 * @returns The parts; undefined when the dialog has no slice.
 */
export const dialogLocationOf = (dialog?: DialogHeaderContext): LocationParts | undefined => dialog?.sliceLocation;
