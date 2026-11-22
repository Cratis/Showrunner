// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Fetching for the release-notes scripts: a request that carries a token only to the places GitHub keeps its own
 * images and API, and a redirect that is followed by hand so the token never travels further than that.
 *
 * A private repository's pull request descriptions refer to images on `private-user-images.githubusercontent.com`,
 * which answer only to a token, and then redirect to a signed address on another host. Without this the note would
 * keep an address that stops working.
 */

export const MAXIMUM_IMAGE_BYTES = 10 * 1024 * 1024;
const REDIRECT_STATUSES = new Set([301, 302, 303, 307, 308]);
const MAXIMUM_REDIRECTS = 5;
const AUTHENTICATED_ORIGINS = new Set([
    'https://api.github.com',
    'https://github.com',
    'https://private-user-images.githubusercontent.com',
]);

/**
 * Builds the headers for a request: the token is added only when the address is one of GitHub's own.
 * @param {string} url - The address being requested.
 * @param {string | undefined} token - The token, when there is one.
 * @returns {Record<string, string>} The headers.
 */
export function requestHeaders(url, token) {
    const headers = { 'User-Agent': 'Cratis-Playbill-release-notes' };
    if (token && AUTHENTICATED_ORIGINS.has(new URL(url).origin)) {
        headers.Authorization = `Bearer ${token}`;
    }
    return headers;
}

async function fetchOnce(url, token, fetchImpl) {
    try {
        return await fetchImpl(url, { headers: requestHeaders(url, token), redirect: 'manual', signal: AbortSignal.timeout(30_000) });
    } catch {
        throw new Error('Release notes fetch failed.');
    }
}

function redirectDestination(response, currentUrl) {
    const location = response.headers.get('location');
    if (!location) {
        throw new Error('Release notes redirect has no destination.');
    }
    const destination = new URL(location, currentUrl);
    if (!['https:', 'http:'].includes(destination.protocol)) {
        throw new Error('Release notes redirect protocol is not allowed.');
    }
    return destination;
}

/**
 * Fetches an address, following redirects by hand and never sending the token to a different origin than the one it was
 * meant for.
 * @param {string} url - The address.
 * @param {string | undefined} token - The token, when there is one.
 * @param {typeof fetch} [fetchImpl] - The fetch to use. Defaults to `fetch`.
 * @returns {Promise<Response>} A successful response.
 * @throws When the request fails, answers with an error status or redirects too often.
 */
export async function fetchResource(url, token, fetchImpl = fetch) {
    for (let redirects = 0; redirects <= MAXIMUM_REDIRECTS; redirects++) {
        const response = await fetchOnce(url, token, fetchImpl);
        if (!REDIRECT_STATUSES.has(response.status)) {
            if (!response.ok) {
                throw new Error(`Release notes fetch failed (HTTP ${response.status}).`);
            }
            return response;
        }

        await response.body?.cancel();
        const destination = redirectDestination(response, url);
        // Once a redirect crosses an origin, never bring the token back later in the chain.
        if (destination.origin !== new URL(url).origin) {
            token = undefined;
        }
        url = destination.href;
    }
    throw new Error('Too many release notes redirects.');
}

/**
 * Reads an image's bytes, refusing one larger than the limit before it is all in memory.
 * @param {Response} response - The response holding the image.
 * @returns {Promise<Buffer>} The bytes.
 * @throws When the image is larger than the limit.
 */
export async function readImageBytes(response) {
    const tooLarge = () => new Error('Release notes image exceeds the 10 MiB limit.');
    if (Number(response.headers.get('content-length')) > MAXIMUM_IMAGE_BYTES) {
        await response.body?.cancel();
        throw tooLarge();
    }
    const chunks = [];
    let size = 0;
    for await (const chunk of response.body) {
        size += chunk.byteLength;
        if (size > MAXIMUM_IMAGE_BYTES) {
            throw tooLarge();
        }
        chunks.push(chunk);
    }
    return Buffer.concat(chunks, size);
}
