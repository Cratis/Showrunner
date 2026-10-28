// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { QueryResult } from '@cratis/arc/queries';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { ErrorDetails } from './ErrorDetails';
import { messageOf } from './messageOf';
import { parseStackTrace } from './parseStackTrace';
import { resolveLocation } from './resolveLocation';

/**
 * Builds {@link ErrorDetails} from the result of a failed Arc query.
 * @remarks
 * A query result carries no correlation id, so pass the one you know, if any. The stack frames are parsed from the
 * stack trace Arc returns, and the type is empty, because Arc does not return the .NET type.
 * @param result The result of the failed query, a {@link QueryResult} or the result of a query hook.
 * @param query The failing query, whose generated proxy declares where it lives.
 * @param location Where it happened when the query's proxy declares nothing, such as the location from
 * `useSliceContext()`.
 * @param correlationId The correlation id of the query, when known.
 * @returns The {@link ErrorDetails}.
 */
export function errorDetailsFromQueryResult(result: Pick<QueryResult<unknown>, 'exceptionMessages' | 'exceptionStackTrace' | 'validationResults'>, query?: object, location?: ArcSliceLocation, correlationId?: Guid): ErrorDetails {
    return {
        message: messageOf(result.exceptionMessages, '', result.validationResults),
        type: '',
        correlationIds: correlationId ? [correlationId] : [],
        stackFrames: parseStackTrace(result.exceptionStackTrace),
        location: resolveLocation(query, location)
    };
}
