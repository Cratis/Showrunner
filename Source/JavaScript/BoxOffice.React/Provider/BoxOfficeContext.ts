// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { PortalIntent, SeatStanding } from '@cratis/showrunner.boxoffice';
import { createContext, useContext } from 'react';
import { BoxOfficeLabels, resolveLabels } from '../Labels/index.js';

/**
 * What {@link BoxOfficeProvider} gives the Box Office components.
 */
export interface BoxOfficeContextValue {
    /** Whether a {@link BoxOfficeProvider} provides the value; the defaults otherwise. */
    readonly isProvided: boolean;

    /** The labels to show. */
    readonly labels: BoxOfficeLabels;

    /** The locale to format dates and numbers for; the browser's when not given. */
    readonly locale?: string;

    /** Where the signed-in user stands with the tenant's seats; nothing while it loads. */
    readonly seat?: SeatStanding;

    /**
     * Opens the customer portal for what the user wants to do there. Reports a failure with a toast.
     * @param intent What the user wants to do.
     * @param offer The offer the user looked at, when they did.
     */
    openPortal(intent: PortalIntent, offer?: Guid): Promise<void>;

    /**
     * Asks the backend again what the tenant may do, for after something changed it.
     */
    refresh(): void;
}

const noop = () => {
    /* Without a BoxOfficeProvider there is nothing to refresh. */
};

/**
 * The value without a {@link BoxOfficeProvider}: English labels, and no portal to open.
 */
export const defaultBoxOfficeContextValue: BoxOfficeContextValue = {
    isProvided: false,
    labels: resolveLabels(),
    openPortal: () => Promise.resolve(),
    refresh: noop
};

/**
 * The React context {@link BoxOfficeProvider} provides.
 */
export const BoxOfficeContext = createContext<BoxOfficeContextValue>(defaultBoxOfficeContextValue);

/**
 * Gets what {@link BoxOfficeProvider} provides, or the defaults outside one.
 * @returns The {@link BoxOfficeContextValue}.
 */
export const useBoxOfficeContext = (): BoxOfficeContextValue => useContext(BoxOfficeContext);

/**
 * Gets the labels to show.
 * @returns The {@link BoxOfficeLabels}.
 */
export const useBoxOfficeLabels = (): BoxOfficeLabels => useContext(BoxOfficeContext).labels;
