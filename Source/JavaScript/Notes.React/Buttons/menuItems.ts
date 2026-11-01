// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { NotesLabels } from '../Labels/index.js';

/**
 * What a drop-down item of the feedback button does.
 */
export type NotesMenuAction = 'slices' | 'screen' | 'component' | 'myFeedback';

/**
 * One item of the drop-down of the feedback button.
 */
export interface NotesMenuItem {
    /** What the item does. */
    readonly action: NotesMenuAction;

    /** The text. */
    readonly label: string;

    /** Whether the item is switched off, such as pointing at something while the user is already pointing. */
    readonly disabled: boolean;
}

/**
 * Gets the items of the drop-down: specific slices, this screen, pointing at a component, and the user's own feedback.
 * @param labels The labels to show.
 * @param isPicking Whether the user is pointing at a component already.
 * @returns The {@link NotesMenuItem}s in the order they are shown.
 */
export function menuItems(labels: NotesLabels, isPicking: boolean): NotesMenuItem[] {
    return [
        { action: 'slices', label: labels.button.aboutSlices, disabled: false },
        { action: 'screen', label: labels.button.aboutScreen, disabled: false },
        { action: 'component', label: labels.button.pointAtSomething, disabled: isPicking },
        { action: 'myFeedback', label: labels.button.myFeedback, disabled: false }
    ];
}

/**
 * Gets which item a key moves to in an open drop-down.
 * @param key The key.
 * @param current The index of the item that has focus; -1 when none has.
 * @param count How many items there are.
 * @returns The index to move focus to; undefined when the key does not move focus.
 */
export function nextMenuIndex(key: string, current: number, count: number): number | undefined {
    switch (key) {
        case 'ArrowDown': return (current + 1) % count;
        case 'ArrowUp': return current <= 0 ? count - 1 : current - 1;
        case 'Home': return 0;
        case 'End': return count - 1;
        default: return undefined;
    }
}
