// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Every text the Notes components show or announce. Placeholders in braces, such as `{count}`, are filled in by the
 * components.
 */
export interface NotesLabels {
    /** Texts shared by several components. */
    common: {
        /** The cancel button. */
        cancel: string;
        /** The accessible name of a dialog's close button. */
        close: string;
        /** Separates the module, feature and slice of a location. */
        locationSeparator: string;
        /** Shown when nothing is known about where the user is. */
        unknownLocation: string;
        /** Introduces what the note is about. */
        aboutHeading: string;
    };

    /** The feedback button and its drop-down. */
    button: {
        /** The text, tooltip and accessible name of the main action. */
        giveFeedback: string;
        /** The accessible name of the drop-down toggle. */
        moreOptions: string;
        /** The drop-down item that gives feedback on specific slices. */
        aboutSlices: string;
        /** The drop-down item that gives feedback on the screen. */
        aboutScreen: string;
        /** The drop-down item that points at a component. */
        pointAtSomething: string;
        /** The drop-down item that opens the user's own feedback. */
        myFeedback: string;
    };

    /** What the feedback dialogs say about where the user is. */
    context: {
        /** The sentence for a location, a screen and no dialog. `{location}` and `{screen}` are filled in. */
        onScreen: string;
        /** The sentence for a location with no screen. `{location}` is filled in. */
        atLocation: string;
        /** The sentence for a dialog. `{location}`, `{dialog}` and `{screen}` are filled in. */
        inDialog: string;
        /** The sentence for a screen only. `{screen}` is filled in. */
        screenOnly: string;
    };

    /** The kinds of feedback. */
    kinds: {
        /** The label of the kind field. */
        title: string;
        /** A problem. */
        problem: string;
        /** A suggestion. */
        suggestion: string;
        /** A question. */
        question: string;
        /** Praise. */
        praise: string;
    };

    /** The dialog that gives feedback on what is on screen, or on the screen. */
    feedback: {
        /** The title when giving feedback on what is on screen. */
        contextTitle: string;
        /** The title when giving feedback on the screen. */
        screenTitle: string;
        /** The send button. */
        send: string;
        /** The label of the text field. */
        text: string;
        /** Confirms that the feedback was sent. */
        sent: string;
    };

    /** The dialog that gives feedback on specific slices. */
    slices: {
        /** The title. */
        title: string;
        /** The label of the list of slices active on the screen. */
        active: string;
        /** The label of the search over all the slices. */
        search: string;
        /** The placeholder of the search. */
        searchPlaceholder: string;
        /** The heading of the search results. */
        results: string;
        /** Shown when nothing matches the search. */
        noMatches: string;
        /** Shown when no slice is active on the screen. */
        noneActive: string;
        /** How many slices are chosen. `{count}` is the number. */
        chosen: string;
        /** The accessible name of a chosen slice's remove button. `{slice}` is the slice. */
        remove: string;
    };

    /** The dialog that gives feedback on a picked component. */
    component: {
        /** The title. */
        title: string;
        /** What the dialog is about. `{component}` is the component. */
        about: string;
        /** The mode that describes the component with text. */
        describe: string;
        /** The mode that changes the component's properties. */
        change: string;
        /** The accessible name of the mode switch. */
        mode: string;
        /** Shown when the component has no properties to change. */
        nothingToChange: string;
        /** Tells that the changes are only a preview until closed. */
        previewNotice: string;
        /** The send button of the change mode. */
        sendChanges: string;
        /** Shown when no property differs. */
        noChanges: string;
        /** Shown when some changes could not be sent. `{count}` is the number. */
        failed: string;
    };

    /** The pointing at a component. */
    picker: {
        /** Announced when pointing starts. */
        started: string;
    };

    /** The user's own feedback. */
    myFeedback: {
        /** The title. */
        title: string;
        /** The description under the title. */
        description: string;
        /** Shown when the user gave no feedback. */
        empty: string;
        /** The withdraw button. */
        withdraw: string;
        /** The accessible name of the withdraw button. `{text}` is the start of the note. */
        withdrawNote: string;
        /** The text of a note that only suggests property changes. */
        suggestionsOnly: string;
        /** What a note is about, by target. */
        targets: {
            /** Feedback on what was on screen. */
            context: string;
            /** Feedback on slices. */
            slices: string;
            /** Feedback on a screen. */
            screen: string;
            /** Feedback on a component. `{component}` is the component. */
            component: string;
        };
        /** The status of a note. */
        statuses: {
            /** Given. */
            given: string;
            /** Acknowledged. */
            acknowledged: string;
            /** Addressed, with the version. `{version}` is the version. */
            addressed: string;
            /** Declined, with the reason. `{reason}` is the reason. */
            declined: string;
            /** Withdrawn. */
            withdrawn: string;
        };
        /** The suggested property changes. `{count}` is the number. */
        propertyChanges: string;
    };
}

/**
 * Makes every property of a type optional, all the way down.
 */
export type DeepPartial<T> = {
    [TKey in keyof T]?: T[TKey] extends object ? DeepPartial<T[TKey]> : T[TKey];
};

/**
 * The labels of the Notes components that a solution replaces, for instance to localize them. Labels left out keep
 * their English defaults.
 */
export type NotesLabelOverrides = DeepPartial<NotesLabels>;
