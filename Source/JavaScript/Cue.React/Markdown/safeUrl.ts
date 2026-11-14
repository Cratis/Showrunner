// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const SAFE_PROTOCOLS = ['https:', 'http:', 'mailto:', 'tel:'];

/**
 * Decides which address a piece of a chapter's Markdown may point at.
 * @param url The address as written.
 * @returns The address when it is safe to follow or show; an empty string when it is not.
 * @remarks
 * A tour's chapters are the application's own writing, but they are still rendered as elements and never as HTML, so an
 * address that runs script is refused. Relative addresses, anchors, bundled images and `data:` images are fine.
 */
export const safeUrl = (url: string): string => {
    const trimmed = url.trim();
    if (trimmed.length === 0) {
        return '';
    }

    if (/^(\/|\.\/|\.\.\/|#|[^:]*$)/.test(trimmed) || /^data:image\/(png|gif|jpeg|webp|svg\+xml)[;,]/i.test(trimmed)) {
        return trimmed;
    }

    try {
        return SAFE_PROTOCOLS.includes(new URL(trimmed).protocol) ? trimmed : '';
    } catch {
        return '';
    }
};
