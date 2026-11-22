// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { isSafeLink } from './safeLink.js';

/**
 * Works out where an image of a release note is loaded from. A note refers to the files captured with it by their
 * name, such as `hero.png`; those are served from the release's own folder. An `https` image the capture could not
 * download is left as it was written. Anything else, such as `javascript:`, `data:` or a path that climbs out of the
 * folder, is not an image.
 * @param source The image as written in the note.
 * @param assetBase Where the files of the release are served from, without a trailing slash; absent when it is not known.
 * @returns The address to load the image from, or an empty string when it is not safe to load.
 */
export const resolveAssetUrl = (source: string, assetBase?: string): string => {
    if (/^https:\/\//i.test(source)) {
        return isSafeLink(source) ? source : '';
    }

    const relative = source.replace(/^\.\//, '');
    const hasScheme = /^[a-z][a-z0-9+.-]*:/i.test(relative) || relative.startsWith('//') || relative.startsWith('/');
    const climbs = relative.split('/').includes('..');
    if (!assetBase || !relative || hasScheme || climbs) {
        return '';
    }

    return `${assetBase}/${relative.split('/').map(encodeURIComponent).join('/')}`;
};
