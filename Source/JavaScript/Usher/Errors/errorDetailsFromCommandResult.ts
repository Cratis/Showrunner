// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ICommandResult } from '@cratis/arc/commands';
import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { ErrorDetails } from './ErrorDetails';
import { messageOf } from './messageOf';
import { parseStackTrace } from './parseStackTrace';
import { resolveLocation } from './resolveLocation';

/**
 * Builds {@link ErrorDetails} from the result of a failed Arc command.
 * @remarks
 * The correlation id is the command's, which lets the backend record the error as it captured it. The stack frames
 * are parsed from the stack trace Arc returns, and the type is empty, because Arc does not return the .NET type.
 * @param result The {@link ICommandResult} of the failed command.
 * @param command The failing command, whose generated proxy declares where it lives.
 * @param location Where it happened when the command's proxy declares nothing, such as the location from
 * `useSliceContext()`.
 * @returns The {@link ErrorDetails}.
 */
export function errorDetailsFromCommandResult(result: ICommandResult<unknown>, command?: object, location?: ArcSliceLocation): ErrorDetails {
    return {
        message: messageOf(result.exceptionMessages, result.authorizationFailureReason, result.validationResults),
        type: '',
        correlationIds: result.correlationId ? [result.correlationId] : [],
        stackFrames: parseStackTrace(result.exceptionStackTrace),
        location: resolveLocation(command, location)
    };
}
