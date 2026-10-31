// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcConfiguration } from '@cratis/arc.react';
import { Guid } from '@cratis/fundamentals';
import { LinkToPortal, PortalIntent } from '@cratis/showrunner.boxoffice';

/**
 * Asks the backend for a signed link that opens the customer portal for what the user wants to do there.
 * @param arc The Arc configuration the application runs with.
 * @param intent What the user wants to do.
 * @param offer The offer the user looked at, when they did.
 * @returns The link; undefined when the backend gave none.
 */
export async function linkToPortal(arc: ArcConfiguration, intent: PortalIntent, offer?: Guid): Promise<string | undefined> {
    const query = new LinkToPortal();
    query.setMicroservice(arc.microservice);
    query.setApiBasePath(arc.apiBasePath ?? '');
    query.setOrigin(arc.origin ?? '');
    query.setHttpHeadersCallback(arc.httpHeadersCallback ?? (() => ({})));

    const result = await query.perform({ intent, offer });
    return result.isSuccess && result.data?.url ? result.data.url : undefined;
}

/**
 * Opens a link in a new tab, without letting the page it opens reach back to this one.
 * @param url The link.
 */
export function openInNewTab(url: string): void {
    window.open(url, '_blank', 'noopener,noreferrer');
}
