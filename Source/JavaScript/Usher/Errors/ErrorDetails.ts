// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { StackFrame } from '../Contracts/Reporting/StackFrame';

/**
 * Represents what is known about an error the user hit, ready for the report dialogs and the `ReportError` command.
 * @remarks
 * Nothing is ever null or undefined: a part that is not known is an empty string, an empty array or
 * {@link ArcSliceLocation.empty}. For a failing command the backend captures the error itself and records that
 * instead of what is sent here, when one of {@link ErrorDetails.correlationIds} is the failing command's.
 */
export interface ErrorDetails {
    /**
     * The message of the error.
     */
    readonly message: string;

    /**
     * The type of the error; empty when the client does not know it, as for a failing command or query, whose
     * .NET type Arc does not return.
     */
    readonly type: string;

    /**
     * The correlation id of the failing command or query; empty when it is not known.
     */
    readonly correlationIds: Guid[];

    /**
     * The stack trace of the error, top frame first; empty when there is none.
     */
    readonly stackFrames: StackFrame[];

    /**
     * Where in the application the error happened.
     */
    readonly location: ArcSliceLocation;
}
