// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Gets whether an address is an absolute `https` or `http` address, the only kind offered to the reporter as a link.
 * Usher already records any other address from a desk as no address; this keeps a link from ever pointing at a
 * `javascript:` or `data:` address should one reach the view some other way.
 * @param address The address.
 * @returns True when it is a web address.
 */
export function isWebAddress(address: string | undefined): boolean {
    if (!address) {
        return false;
    }

    try {
        const { protocol } = new URL(address);
        return protocol === 'https:' || protocol === 'http:';
    } catch {
        return false;
    }
}
