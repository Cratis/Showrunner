// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ApplicationContextSnapshot } from '@cratis/arc.react';
import { Guid } from '@cratis/fundamentals';
import { NoteContext, NoteDialog, NoteScreen, SliceLocation } from '@cratis/showrunner.notes';
import { format, NotesLabels } from '../Labels/index.js';
import { formatLocation, LocationParts } from './formatLocation.js';

/**
 * The key of the Arc application context contributions that holds the identifiers of model elements.
 */
export const modelReferencesKey = 'modelReferences';

/**
 * The dialog a note is given from.
 */
export interface DialogReference {
    /** The title of the dialog. */
    readonly title: string;

    /** Where the dialog belongs. */
    readonly location: LocationParts;
}

/**
 * What {@link buildNoteContext} reads besides the application context.
 */
export interface NoteContextInput {
    /** What the user is looking at, from Arc's `useApplicationContext()`. */
    readonly snapshot: ApplicationContextSnapshot;

    /** The route of the browser. */
    readonly route: string;

    /** The correlation id to send. */
    readonly correlationId: Guid;

    /** The dialog the note is given from; the topmost open dialog of the snapshot when not given. */
    readonly dialog?: DialogReference;
}

/**
 * Makes a location of the contract from the parts of one.
 * @param parts The parts; an absent part is empty.
 * @returns The {@link SliceLocation}.
 */
export function toSliceLocation(parts?: LocationParts): SliceLocation {
    return Object.assign(new SliceLocation(), {
        module: parts?.module ?? '',
        feature: parts?.feature ?? '',
        slice: parts?.slice ?? ''
    });
}

/**
 * Reads the identifiers of model elements the host contributed to the Arc application context.
 * @param contributions The contributions of the application context.
 * @returns The identifiers by what they identify; empty when the host contributed none.
 */
export function modelReferencesOf(contributions: Readonly<Record<string, unknown>>): Record<string, string> {
    const value = contributions[modelReferencesKey];
    if (typeof value !== 'object' || value === null || Array.isArray(value)) {
        return {};
    }

    return Object.fromEntries(Object.entries(value)
        .filter(([, reference]) => reference !== undefined && reference !== null)
        .map(([key, reference]) => [key, String(reference)]));
}

/**
 * Builds the context every Notes command carries from what the user is looking at.
 * @param input The {@link NoteContextInput}.
 * @returns The {@link NoteContext}; when a dialog is the subject its location is the focused location.
 */
export function buildNoteContext(input: NoteContextInput): NoteContext {
    const { snapshot } = input;
    const dialog = input.dialog ?? openDialogOf(snapshot);

    return Object.assign(new NoteContext(), {
        location: toSliceLocation(dialog?.location ?? snapshot.location),
        activeSlices: snapshot.activeSlices.map(toSliceLocation),
        screen: Object.assign(new NoteScreen(), {
            screenId: snapshot.screen.id,
            screenName: snapshot.screen.name,
            screenPath: snapshot.screen.path
        }),
        route: input.route,
        dialog: Object.assign(new NoteDialog(), {
            dialogTitle: dialog?.title ?? '',
            location: toSliceLocation(dialog?.location)
        }),
        modelReferences: modelReferencesOf(snapshot.contributions),
        correlationId: input.correlationId
    });
}

function openDialogOf(snapshot: ApplicationContextSnapshot): DialogReference | undefined {
    const dialog = snapshot.dialogs.at(-1);
    return dialog ? { title: dialog.title ?? '', location: dialog.location } : undefined;
}

/**
 * Says in plain language what a note context is about, such as `Orders › Editing › Change order, on screen Order details`.
 * @param context The context.
 * @param labels The labels to show.
 * @returns The sentence.
 */
export function describeContext(context: NoteContext, labels: NotesLabels): string {
    const location = formatLocation(context.location, labels.common.locationSeparator, labels.common.unknownLocation);
    const screen = context.screen.screenName || context.screen.screenPath;
    const dialog = context.dialog.dialogTitle;

    if (dialog) {
        return format(labels.context.inDialog, { location, dialog, screen: screen || labels.common.unknownLocation });
    }

    if (screen && !isEmpty(context.location)) {
        return format(labels.context.onScreen, { location, screen });
    }

    return screen ? format(labels.context.screenOnly, { screen }) : format(labels.context.atLocation, { location });
}

/**
 * Says in plain language where a screen note is given.
 * @param context The context.
 * @param labels The labels to show.
 * @returns The sentence.
 */
export function describeScreen(context: NoteContext, labels: NotesLabels): string {
    const screen = context.screen.screenName || context.screen.screenPath;
    return format(labels.context.screenOnly, { screen: screen || labels.common.unknownLocation });
}

function isEmpty(location: LocationParts): boolean {
    return !location.module && !location.feature && !location.slice;
}
