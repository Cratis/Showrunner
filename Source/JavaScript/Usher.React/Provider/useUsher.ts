// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useSliceContext } from '@cratis/arc.react/slices';
import { Guid } from '@cratis/fundamentals';
import { useMemo } from 'react';
import { UsherLabels } from '../Labels/index.js';
import { useUsherContext } from './UsherContext.js';

/**
 * What {@link useUsher} gives a component.
 */
export interface Usher {
    /** The labels Usher shows. */
    readonly labels: UsherLabels;

    /**
     * Opens the report a problem dialog, with where in the application the component is.
     */
    reportIssue(): void;

    /**
     * Opens the report this error dialog.
     * @param error The error: `ErrorDetails` from `@cratis/showrunner.usher`, such as `errorDetailsFromCommandResult`
     * gives for a failed command, or anything thrown.
     */
    reportError(error: unknown): void;

    /**
     * Opens the my issues side sheet.
     */
    showMyIssues(): void;

    /**
     * Opens the my issues side sheet on one issue.
     * @param issueId The issue.
     */
    showIssue(issueId: Guid): void;

    /**
     * Gets the correlation ids of the last few Arc commands and queries run, oldest first.
     * @returns The correlation ids.
     */
    recentCorrelationIds(): Guid[];

    /**
     * Tracks the correlation id of a command or query, such as one from a `CommandResult` of a command run outside
     * the application's `<Arc>` context.
     * @param correlationId The correlation id.
     */
    trackCorrelationId(correlationId: Guid | string): void;
}

/**
 * Uses Usher from any component inside an {@link UsherProvider}. Reports carry the slice location of the component
 * that calls this hook.
 * @returns The {@link Usher}.
 */
export function useUsher(): Usher {
    const context = useUsherContext();
    const location = useSliceContext();

    return useMemo(() => ({
        labels: context.labels,
        reportIssue: () => context.reportIssue(location),
        reportError: (error: unknown) => context.reportError(error, location),
        showMyIssues: () => context.showMyIssues(location),
        showIssue: (issueId: Guid) => context.showIssue(issueId),
        recentCorrelationIds: () => context.recentCorrelationIds(),
        trackCorrelationId: (correlationId: Guid | string) => context.trackCorrelationId(correlationId)
    }), [context, location]);
}
