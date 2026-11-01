// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { NotesLabels } from './NotesLabels.js';

/**
 * The English labels of the Notes components.
 */
export const defaultNotesLabels: NotesLabels = {
    common: {
        cancel: 'Cancel',
        close: 'Close',
        locationSeparator: ' › ',
        unknownLocation: 'Unknown location',
        aboutHeading: 'About'
    },
    button: {
        giveFeedback: 'Give feedback',
        moreOptions: 'More ways to give feedback',
        aboutSlices: 'About specific slices…',
        aboutScreen: 'About this screen…',
        pointAtSomething: 'Point at something…',
        myFeedback: 'My feedback'
    },
    context: {
        onScreen: '{location}, on screen {screen}',
        atLocation: '{location}',
        inDialog: '{location}, in the dialog {dialog}, on screen {screen}',
        screenOnly: 'Screen {screen}'
    },
    kinds: {
        title: 'What kind of feedback is it?',
        problem: 'Problem',
        suggestion: 'Suggestion',
        question: 'Question',
        praise: 'Praise'
    },
    feedback: {
        contextTitle: 'Give feedback on this',
        screenTitle: 'Give feedback on this screen',
        send: 'Send feedback',
        text: 'What do you think?',
        sent: 'Thank you for your feedback'
    },
    slices: {
        title: 'Give feedback on specific slices',
        active: 'On this screen',
        search: 'Search all slices',
        searchPlaceholder: 'Search by module, feature or slice',
        results: 'Matching slices',
        noMatches: 'No slice matches.',
        noneActive: 'No slice is active on this screen.',
        chosen: '{count} chosen',
        remove: 'Remove {slice}'
    },
    component: {
        title: 'Give feedback on a component',
        about: 'About {component}',
        describe: 'Describe it',
        change: 'Change it',
        mode: 'How to give feedback',
        nothingToChange: 'This component has no properties to change.',
        previewNotice: 'Changes show here as a preview, and go away when you close this dialog.',
        sendChanges: 'Send changes',
        noChanges: 'Change a property to send a suggestion.',
        failed: '{count} changes could not be sent. Try again.'
    },
    picker: {
        started: 'Click a component to give feedback on it. Press Escape to cancel.'
    },
    myFeedback: {
        title: 'My feedback',
        description: 'The feedback you gave and where it stands.',
        empty: 'You have not given any feedback yet.',
        withdraw: 'Withdraw',
        withdrawNote: 'Withdraw the note: {text}',
        suggestionsOnly: 'Suggested changes',
        targets: {
            context: 'What was on screen',
            slices: 'Slices',
            screen: 'Screen',
            component: 'Component {component}'
        },
        statuses: {
            given: 'Given',
            acknowledged: 'Acknowledged',
            addressed: 'Addressed in {version}',
            declined: 'Declined: {reason}',
            withdrawn: 'Withdrawn'
        },
        propertyChanges: '{count} suggested property changes'
    }
};
