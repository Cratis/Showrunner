// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { format, UsherLabels } from '../Labels/index.js';

/**
 * The sizes of the Usher buttons: `large` shows the icon and the text; `small` shows the icon, with the text as the
 * tooltip and the accessible name.
 */
export type UsherButtonSize = 'large' | 'small';

/**
 * What an Usher button shows and announces.
 */
export interface ButtonModel {
    /** The visible text; empty for a small button. */
    readonly text: string;

    /** The accessible name. */
    readonly accessibleName: string;

    /** The tooltip; empty for a large button, whose text says it all. */
    readonly tooltip: string;
}

/**
 * Gets what the report button shows and announces.
 * @param size The {@link UsherButtonSize}.
 * @param reportsError Whether the button reports an error rather than a problem described by hand.
 * @param labels The labels to show.
 * @returns The {@link ButtonModel}.
 */
export function reportButtonModel(size: UsherButtonSize, reportsError: boolean, labels: UsherLabels): ButtonModel {
    const label = reportsError ? labels.reportButton.reportError : labels.reportButton.reportProblem;
    return modelFor(size, label, label);
}

/**
 * Gets what the my issues button shows and announces.
 * @param size The {@link UsherButtonSize}.
 * @param waiting How many issues support waits for the user on.
 * @param labels The labels to show.
 * @returns The {@link ButtonModel}.
 */
export function myIssuesButtonModel(size: UsherButtonSize, waiting: number, labels: UsherLabels): ButtonModel {
    const label = labels.myIssuesButton.label;
    const accessibleName = waiting > 0 ? `${label}, ${format(labels.myIssuesButton.waiting, { count: waiting })}` : label;
    return modelFor(size, label, accessibleName);
}

function modelFor(size: UsherButtonSize, label: string, accessibleName: string): ButtonModel {
    return size === 'small'
        ? { text: '', accessibleName, tooltip: accessibleName }
        : { text: label, accessibleName, tooltip: '' };
}
