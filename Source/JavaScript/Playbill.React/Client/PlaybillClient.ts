// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CurrentRelease, Release, ReleaseList } from './models.js';

/**
 * Where Playbill serves the release history from, relative to the origin of the application.
 */
export const defaultBasePath = '/.cratis/playbill/v1';

/**
 * Reads the release history an application serves with `MapPlaybill`. Give your own to {@link PlaybillProvider} to change
 * where the history is read from or how the request is made.
 */
export class PlaybillClient {
    /**
     * Initializes a new instance of the {@link PlaybillClient} class.
     * @param basePath Where the history is served from. Defaults to `/.cratis/playbill/v1`.
     * @param fetcher Makes the request. Defaults to `fetch`; give one to add headers or point at another origin.
     */
    constructor(
        readonly basePath: string = defaultBasePath,
        private readonly fetcher: typeof fetch = (input, init) => fetch(input, init)) {
    }

    /**
     * Lists the releases, the newest first.
     * @param since When set, only the releases newer than this version.
     * @param signal Cancels the request.
     * @returns The list.
     */
    list(since?: string, signal?: AbortSignal): Promise<ReleaseList> {
        const query = since ? `?since=${encodeURIComponent(since)}` : '';
        return this.read<ReleaseList>(`/releases${query}`, signal);
    }

    /**
     * Gets one release with its note.
     * @param version The version of the release.
     * @param signal Cancels the request.
     * @returns The release.
     */
    get(version: string, signal?: AbortSignal): Promise<Release> {
        return this.read<Release>(`/releases/${encodeURIComponent(version)}`, signal);
    }

    /**
     * Gets the version the application is running and the newest release that has a note.
     * @param signal Cancels the request.
     * @returns The current release.
     */
    current(signal?: AbortSignal): Promise<CurrentRelease> {
        return this.read<CurrentRelease>('/current', signal);
    }

    /**
     * Gets the address the files of a release are served from: an image the note refers to as `hero.png` is at
     * `<this address>/hero.png`.
     * @param version The version of the release.
     * @returns The address, without a trailing slash.
     */
    assetBase(version: string): string {
        return `${this.basePath}/releases/${encodeURIComponent(version)}`;
    }

    private async read<TResult>(path: string, signal?: AbortSignal): Promise<TResult> {
        const response = await this.fetcher(`${this.basePath}${path}`, {
            headers: { Accept: 'application/json' },
            credentials: 'same-origin',
            signal
        });
        if (!response.ok) {
            throw new Error(`Playbill could not read ${path}: ${response.status} ${response.statusText}`.trim());
        }

        return await response.json() as TResult;
    }
}
