// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The most the backend accepts in one report, as `ReportLimits` in the Usher backend states it. What the application
 * itself gathers - stack frames, correlation ids and the error's own texts - is cut to these before a report is sent,
 * so a long stack trace never makes a report fail; what the reporter writes is checked by the generated validators.
 */
export enum ReportLimits {
    /** The longest error message a report carries. */
    errorMessageLength = 2000,

    /** The longest error type a report carries. */
    errorTypeLength = 500,

    /** The most stack frames a report carries. */
    stackFrames = 50,

    /** The longest method or file of a stack frame. */
    stackFrameTextLength = 1000,

    /** The most correlation ids a report carries. */
    correlationIds = 20
}
