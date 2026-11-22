// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback } from 'react';
import { Release, ReleaseSummary } from './Client/index.js';
import { usePlaybillClient } from './Configuration/index.js';
import { Loaded, useLoaded } from './useLoaded.js';

/**
 * What {@link useReleases} gives a component.
 */
export interface UsedReleases {
    /** The releases, the newest first; empty until they are read. */
    releases: ReleaseSummary[];
    /** The version the application is running; absent when it is not known. */
    current?: string;
    /** Whether the releases are being read. */
    isLoading: boolean;
    /** Why they could not be read; absent when they could. */
    error?: Error;
}

/**
 * Reads the list of releases the application serves. Needs nothing but the endpoints `MapPlaybill` maps, so it works
 * without Arc and for a person who has not signed in when the application allows anonymous callers.
 * @returns The releases, whether they are being read and why they could not be.
 */
export const useReleases = (): UsedReleases => {
    const client = usePlaybillClient();
    const load = useCallback((signal: AbortSignal) => client.list(undefined, signal), [client]);
    const { data, isLoading, error } = useLoaded(load);
    return { releases: data?.releases ?? [], current: data?.current ?? undefined, isLoading, error };
};

/**
 * Reads one release with its note.
 * @param version The version of the release; nothing is read while it is absent.
 * @returns The release, whether it is being read and why it could not be.
 */
export const useRelease = (version?: string): Loaded<Release> => {
    const client = usePlaybillClient();
    const load = useCallback((signal: AbortSignal) => version
        ? client.get(version, signal)
        : Promise.reject(new Error('No release was asked for.')), [client, version]);
    const loaded = useLoaded(load);
    return version ? loaded : { isLoading: false };
};

/**
 * Reads several releases with their notes, in the order they are asked for.
 * @param versions The versions of the releases.
 * @returns The releases, whether they are being read and why they could not be.
 */
export const useReleaseNotes = (versions: string[]): Loaded<Release[]> => {
    const client = usePlaybillClient();
    const key = versions.join('|');
    const load = useCallback((signal: AbortSignal) => Promise.all(key === '' ? [] : key.split('|').map(version => client.get(version, signal))), [client, key]);
    return useLoaded(load);
};
