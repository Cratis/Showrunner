// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { GetHttpHeaders } from '@cratis/arc';
import { Guid } from '@cratis/fundamentals';
import { CorrelationTracker } from './CorrelationTracker.js';

/**
 * The HTTP header Arc reads the correlation id of a command or query from, unless the backend configures another.
 */
export const defaultCorrelationIdHeader = 'X-Correlation-ID';

/**
 * Wraps the HTTP headers callback Arc calls for every command and query request, so each request carries a
 * correlation id the tracker knows.
 * @remarks
 * Arc uses the correlation id a request carries in its correlation id header, so the id tracked is the id the
 * backend records for the command or query. A correlation id the headers already carry is kept and tracked;
 * otherwise a new one is added.
 * @param headersCallback The callback to wrap; nothing for none.
 * @param tracker The {@link CorrelationTracker} to track the correlation ids in.
 * @param header The name of the correlation id header.
 * @param createCorrelationId Creates a new correlation id.
 * @returns The wrapping callback.
 */
export function withCorrelationIdHeader(
    headersCallback: GetHttpHeaders | undefined,
    tracker: CorrelationTracker,
    header: string = defaultCorrelationIdHeader,
    createCorrelationId: () => Guid = () => Guid.create()): GetHttpHeaders {
    return () => {
        const headers = new Headers(headersCallback?.() ?? {});
        const existing = headers.get(header);
        const correlationId = existing && Guid.isGuid(existing) ? existing : createCorrelationId().toString();
        headers.set(header, correlationId);
        tracker.track(correlationId);

        const record: Record<string, string> = {};
        headers.forEach((value, name) => {
            record[name] = value;
        });
        return record;
    };
}
