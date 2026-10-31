// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const allowedProtocols = new Set(['https:', 'mailto:', 'tel:']);

/**
 * Whether a link may be followed from an announcement: only `https`, `mailto` and `tel`. A relative link, a scheme
 * that runs code such as `javascript:` and a scheme that carries content such as `data:` are not.
 * @param url The link as written in the announcement.
 * @returns True when it is safe to link to.
 */
export const isSafeLink = (url: string): boolean => {
    try {
        return allowedProtocols.has(new URL(url).protocol);
    } catch {
        return false;
    }
};

/**
 * Empties a link that is not safe, so it never reaches an `href`.
 * @param url The link as written in the announcement.
 * @returns The link, or an empty string.
 */
export const safeLinkOrEmpty = (url: string): string => isSafeLink(url) ? url : '';
