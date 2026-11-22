// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { createContext, ReactNode, useContext, useMemo } from 'react';
import { PlaybillClient } from '../Client/index.js';

const defaultClient = new PlaybillClient();

const PlaybillContext = createContext<PlaybillClient>(defaultClient);

/**
 * Props for {@link PlaybillProvider}.
 */
export interface PlaybillProviderProps {
    /** Where the history is served from. Defaults to `/.cratis/playbill/v1`. Ignored when a `client` is given. */
    basePath?: string;
    /** Makes the requests, for adding headers or reading another origin. Defaults to `fetch`. Ignored when a `client` is given. */
    fetcher?: typeof fetch;
    /** Reads the history instead of the default client. */
    client?: PlaybillClient;
    /** What reads the history. */
    children: ReactNode;
}

/**
 * Tells the Playbill components where to read the release history from. It is optional: without it they read
 * `/.cratis/playbill/v1` of the page's own origin, which is where `MapPlaybill` serves it.
 */
export const PlaybillProvider = ({ basePath, fetcher, client, children }: PlaybillProviderProps) => {
    const value = useMemo(() => client ?? new PlaybillClient(basePath, fetcher), [client, basePath, fetcher]);
    return <PlaybillContext.Provider value={value}>{children}</PlaybillContext.Provider>;
};

/**
 * Gets the client that reads the release history.
 * @returns The client of the nearest {@link PlaybillProvider}, or the default one.
 */
export const usePlaybillClient = (): PlaybillClient => useContext(PlaybillContext);
