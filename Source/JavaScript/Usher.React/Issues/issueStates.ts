// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TagSeverity } from '@cratis/components/Display';
import { IssueState } from '@cratis/showrunner.usher';
import { UsherLabels } from '../Labels/index.js';

/**
 * How the state of an issue is shown.
 */
export interface StateBadge {
    /** The text. */
    readonly label: string;

    /** The severity of the tag that shows it. */
    readonly severity: TagSeverity;
}

/**
 * Gets how the state of an issue is shown.
 * @param state The {@link IssueState}.
 * @param labels The labels to show.
 * @returns The {@link StateBadge}.
 */
export function stateBadgeFor(state: IssueState, labels: UsherLabels): StateBadge {
    switch (state) {
        case IssueState.reported:
            return { label: labels.states.reported, severity: 'secondary' };
        case IssueState.triaged:
            return { label: labels.states.triaged, severity: 'info' };
        case IssueState.accepted:
            return { label: labels.states.accepted, severity: 'info' };
        case IssueState.inProgress:
            return { label: labels.states.inProgress, severity: 'warn' };
        case IssueState.resolved:
            return { label: labels.states.resolved, severity: 'success' };
        case IssueState.wontFix:
            return { label: labels.states.wontFix, severity: 'contrast' };
        case IssueState.withdrawn:
            return { label: labels.states.withdrawn, severity: 'secondary' };
        default:
            return { label: labels.states.reported, severity: 'secondary' };
    }
}

/**
 * Checks whether an issue is closed: resolved, won't fix or withdrawn.
 * @param state The {@link IssueState}.
 * @returns True when closed.
 */
export function isClosed(state: IssueState): boolean {
    return state === IssueState.resolved || state === IssueState.wontFix || state === IssueState.withdrawn;
}

/**
 * The steps of the status strip of an issue, in order.
 */
export const statusSteps: IssueState[] = [IssueState.reported, IssueState.triaged, IssueState.accepted, IssueState.inProgress, IssueState.resolved];

/**
 * Gets how far along the status strip an issue is.
 * @param state The {@link IssueState}.
 * @returns The index of the step reached; the last step for an issue support will not fix or the reporter
 * withdrew, as nothing more happens on it.
 */
export function statusStepOf(state: IssueState): number {
    const index = statusSteps.indexOf(state);
    return index >= 0 ? index : statusSteps.length - 1;
}
