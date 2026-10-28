// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { ErrorDetails } from './ErrorDetails';
import { parseStackTrace } from './parseStackTrace';

/**
 * Builds {@link ErrorDetails} from a JavaScript error, or anything else that was thrown.
 * @remarks
 * The type is the error's `name`, or the name of its class when a subclass leaves `name` as `Error`. A thrown
 * value that is not an `Error` has no type and no stack frames, and its text as the message.
 * @param error What was thrown.
 * @param location Where it happened, such as the location from `useSliceContext()`.
 * @returns The {@link ErrorDetails}.
 */
export function errorDetailsFromError(error: unknown, location?: ArcSliceLocation): ErrorDetails {
    if (error instanceof Error) {
        return {
            message: error.message,
            type: typeOf(error),
            correlationIds: [],
            stackFrames: parseStackTrace(error.stack),
            location: location ?? ArcSliceLocation.empty
        };
    }

    return {
        message: error === undefined ? '' : String(error),
        type: '',
        correlationIds: [],
        stackFrames: [],
        location: location ?? ArcSliceLocation.empty
    };
}

function typeOf(error: Error): string {
    const className = error.constructor?.name ?? '';
    if (error.name === 'Error' && className.length > 0) {
        return className;
    }

    return error.name;
}
