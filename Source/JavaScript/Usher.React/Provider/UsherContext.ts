// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcConfiguration } from '@cratis/arc.react';
import { SliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { createContext, useContext } from 'react';
import { resolveLabels, UsherLabels } from '../Labels/index.js';

/**
 * What {@link UsherProvider} gives the Usher components.
 */
export interface UsherContextValue {
    /** Whether an {@link UsherProvider} provides the value; the defaults otherwise. */
    readonly isProvided: boolean;

    /** The labels to show. */
    readonly labels: UsherLabels;

    /** The name of the application, shown with what a report sends; empty when not given. */
    readonly applicationName: string;

    /** The version of the application, sent with every report; empty when not given. */
    readonly applicationVersion: string;

    /** Whether the report dialogs let support know who the reporter is unless they choose otherwise. */
    readonly shareIdentityByDefault: boolean;

    /** The locale to format dates and times for; the browser's when not given. */
    readonly locale?: string;

    /** The Arc configuration without correlation tracking, for Usher's own commands and queries. */
    readonly untrackedArc?: ArcConfiguration;

    /**
     * Gets the correlation ids of the last few Arc commands and queries run, oldest first.
     * @returns The correlation ids.
     */
    recentCorrelationIds(): Guid[];

    /**
     * Tracks the correlation id of a command or query run, such as one from a `CommandResult`.
     * @param correlationId The correlation id.
     */
    trackCorrelationId(correlationId: Guid | string): void;

    /**
     * Opens the report a problem dialog.
     * @param location Where in the application the reporter is.
     */
    reportIssue(location: SliceLocation): void;

    /**
     * Opens the report this error dialog.
     * @param error The error: `ErrorDetails` from `@cratis/showrunner.usher`, or anything thrown.
     * @param location Where in the application the reporter is.
     */
    reportError(error: unknown, location: SliceLocation): void;

    /**
     * Opens the my issues side sheet with the list of issues.
     * @param location Where in the application the user is, for a problem they report from the side sheet.
     */
    showMyIssues(location: SliceLocation): void;

    /**
     * Opens the my issues side sheet on one issue.
     * @param issueId The issue.
     */
    showIssue(issueId: Guid): void;
}

const noop = () => {
    /* Without an UsherProvider there are no dialogs to open. */
};

/**
 * The value without an {@link UsherProvider}: English labels, and nothing to open.
 */
export const defaultUsherContextValue: UsherContextValue = {
    isProvided: false,
    labels: resolveLabels(),
    applicationName: '',
    applicationVersion: '',
    shareIdentityByDefault: false,
    recentCorrelationIds: () => [],
    trackCorrelationId: noop,
    reportIssue: noop,
    reportError: noop,
    showMyIssues: noop,
    showIssue: noop
};

/**
 * The React context {@link UsherProvider} provides.
 */
export const UsherContext = createContext<UsherContextValue>(defaultUsherContextValue);

/**
 * Gets what {@link UsherProvider} provides, or the defaults outside one.
 * @returns The {@link UsherContextValue}.
 */
export const useUsherContext = (): UsherContextValue => useContext(UsherContext);

/**
 * Gets the labels to show.
 * @returns The {@link UsherLabels}.
 */
export const useUsherLabels = (): UsherLabels => useContext(UsherContext).labels;
