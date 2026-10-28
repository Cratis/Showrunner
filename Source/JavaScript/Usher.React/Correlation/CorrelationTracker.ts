// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';

/**
 * Keeps the correlation ids of the last few Arc commands and queries, so a report can carry them.
 */
export class CorrelationTracker {
    private _correlationIds: Guid[] = [];

    /**
     * Initializes a new instance of {@link CorrelationTracker}.
     * @param capacity How many correlation ids to keep; the oldest is dropped when one more is tracked.
     */
    constructor(readonly capacity: number = 5) {
    }

    /**
     * Gets the correlation ids kept, oldest first.
     */
    get correlationIds(): Guid[] {
        return [...this._correlationIds];
    }

    /**
     * Tracks a correlation id. A correlation id already kept moves to the end instead of being kept twice; an
     * empty one is ignored.
     * @param correlationId The correlation id, as a {@link Guid} or its string form.
     */
    track(correlationId: Guid | string): void {
        const parsed = typeof correlationId === 'string' ? parseGuid(correlationId) : correlationId;
        if (!parsed || parsed.equals(Guid.empty)) {
            return;
        }

        this._correlationIds = [...this._correlationIds.filter(existing => !existing.equals(parsed)), parsed]
            .slice(-Math.max(this.capacity, 0));
    }

    /**
     * Forgets every correlation id kept.
     */
    clear(): void {
        this._correlationIds = [];
    }
}

function parseGuid(value: string): Guid | undefined {
    return Guid.isGuid(value) ? Guid.parse(value) : undefined;
}
