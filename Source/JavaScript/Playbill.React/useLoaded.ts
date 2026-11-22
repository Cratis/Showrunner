// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useEffect, useState } from 'react';

/**
 * What {@link useLoaded} gives a component.
 */
export interface Loaded<TData> {
    /** What was read; absent until the first read has finished, and while the read is repeated for another input. */
    data?: TData;
    /** Whether a read is in progress. */
    isLoading: boolean;
    /** Why the last read failed; absent when it did not. */
    error?: Error;
}

/**
 * Reads something when the component is shown and again whenever the read changes, cancelling a read that is no longer
 * wanted. The read must be stable between renders, for example made with `useCallback`.
 * @param load Starts the read; gets a signal that is aborted when it is no longer wanted.
 * @returns What was read, whether it is still being read, and why it failed.
 */
export const useLoaded = <TData,>(load: (signal: AbortSignal) => Promise<TData>): Loaded<TData> => {
    const [state, setState] = useState<Loaded<TData>>({ isLoading: true });

    useEffect(() => {
        const controller = new AbortController();
        setState(previous => ({ isLoading: true, data: previous.data }));
        load(controller.signal)
            .then(data => {
                if (!controller.signal.aborted) {
                    setState({ isLoading: false, data });
                }
            })
            .catch((error: unknown) => {
                if (!controller.signal.aborted) {
                    setState({ isLoading: false, error: error instanceof Error ? error : new Error(String(error)) });
                }
            });
        return () => controller.abort();
    }, [load]);

    return state;
};
