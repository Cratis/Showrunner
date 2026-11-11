// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { Guid } from '@cratis/fundamentals';
import { AccessDuration, ErrorDetails, errorDetailsFromError, IReportError, IReportIssue, SliceLocation, StackFrame } from '@cratis/showrunner.usher';
import { ReportLimits } from './ReportLimits.js';

/**
 * What every report carries besides what the reporter writes.
 */
export interface ReportContext {
    /** The id of the new issue. */
    readonly issueId: Guid;

    /** Where in the application the reporter was. */
    readonly location: ArcSliceLocation;

    /** The correlation ids of the commands and queries run just before the report. */
    readonly correlationIds: Guid[];

    /** The version of the application. */
    readonly applicationVersion: string;

    /** Whether to let support know who the reporter is unless they choose otherwise. */
    readonly shareIdentity: boolean;
}

/**
 * Converts where something happened to the location a report carries.
 * @param location The {@link ArcSliceLocation}, as `useSliceContext()` or a generated proxy gives it.
 * @returns The {@link SliceLocation} of the report.
 */
export function toReportLocation(location: ArcSliceLocation): SliceLocation {
    const reportLocation = new SliceLocation();
    reportLocation.module = location.module;
    reportLocation.feature = location.feature;
    reportLocation.slice = location.slice;
    return reportLocation;
}

/**
 * Gets the initial values of the report a problem command.
 * @param context The {@link ReportContext}.
 * @returns The values.
 */
export function reportIssueValues(context: ReportContext): IReportIssue {
    return {
        issueId: context.issueId,
        summary: '',
        description: '',
        location: toReportLocation(context.location),
        correlationIds: context.correlationIds.slice(-ReportLimits.correlationIds),
        applicationVersion: context.applicationVersion,
        shareIdentity: context.shareIdentity,
        grantAccess: false,
        accessDuration: AccessDuration.oneDay
    };
}

/**
 * Gets the initial values of the report this error command. The error's own location and correlation ids win over
 * those of the context, as they say where the error happened.
 * @param error The {@link ErrorDetails}.
 * @param context The {@link ReportContext}.
 * @returns The values.
 */
export function reportErrorValues(error: ErrorDetails, context: ReportContext): IReportError {
    return {
        issueId: context.issueId,
        errorMessage: error.message.slice(0, ReportLimits.errorMessageLength),
        errorType: error.type.slice(0, ReportLimits.errorTypeLength),
        stackFrames: error.stackFrames.slice(0, ReportLimits.stackFrames).map(withinLimits),
        details: '',
        location: toReportLocation(isEmptyLocation(error.location) ? context.location : error.location),
        correlationIds: mergeCorrelationIds(error.correlationIds, context.correlationIds).slice(0, ReportLimits.correlationIds),
        applicationVersion: context.applicationVersion,
        shareIdentity: context.shareIdentity,
        grantAccess: false,
        accessDuration: AccessDuration.oneDay
    };
}

/**
 * Merges correlation ids, keeping each once, the first given first.
 * @param first The correlation ids that come first.
 * @param second The correlation ids that come after.
 * @returns The merged correlation ids.
 */
export function mergeCorrelationIds(first: Guid[], second: Guid[]): Guid[] {
    const merged: Guid[] = [];
    for (const correlationId of [...first, ...second]) {
        if (!merged.some(existing => existing.equals(correlationId))) {
            merged.push(correlationId);
        }
    }
    return merged;
}

/**
 * Gets the {@link ErrorDetails} of an error to report: the details themselves when given, otherwise the details of
 * what was thrown.
 * @param error `ErrorDetails`, or anything thrown.
 * @param location Where it happened, when what was thrown does not say.
 * @returns The {@link ErrorDetails}.
 */
export function toErrorDetails(error: unknown, location?: ArcSliceLocation): ErrorDetails {
    return isErrorDetails(error) ? error : errorDetailsFromError(error, location);
}

/**
 * Checks whether a value is {@link ErrorDetails}.
 * @param value The value.
 * @returns True when it is.
 */
export function isErrorDetails(value: unknown): value is ErrorDetails {
    return isObjectThatIsNotAnError(value)
        && hasTextMembers(value)
        && hasListMembers(value)
        && hasLocation(value);
}

/**
 * Checks whether a value is an object that is not an error.
 * @param value The value.
 * @returns True when it is.
 */
function isObjectThatIsNotAnError(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null && !(value instanceof Error);
}

/**
 * Checks whether the message and the type of a candidate are text.
 * @param candidate The candidate.
 * @returns True when they are.
 */
function hasTextMembers(candidate: Record<string, unknown>): boolean {
    return typeof candidate.message === 'string' && typeof candidate.type === 'string';
}

/**
 * Checks whether the correlation ids and the stack frames of a candidate are lists.
 * @param candidate The candidate.
 * @returns True when they are.
 */
function hasListMembers(candidate: Record<string, unknown>): boolean {
    return Array.isArray(candidate.correlationIds) && Array.isArray(candidate.stackFrames);
}

/**
 * Checks whether a candidate has a location.
 * @param candidate The candidate.
 * @returns True when it does.
 */
function hasLocation(candidate: Record<string, unknown>): boolean {
    return typeof candidate.location === 'object' && candidate.location !== null;
}

/**
 * Checks whether a location says nothing about where something happened.
 * @param location The location.
 * @returns True when no part of it is known.
 */
export function isEmptyLocation(location: { module: string; feature: string; slice: string } | undefined): boolean {
    return !location || [location.module, location.feature, location.slice].every(part => !part);
}

/**
 * Cuts the texts of a stack frame to what a report carries.
 * @param frame The {@link StackFrame}.
 * @returns The frame within the limits.
 */
function withinLimits(frame: StackFrame): StackFrame {
    return {
        ...frame,
        method: frame.method.slice(0, ReportLimits.stackFrameTextLength),
        file: frame.file.slice(0, ReportLimits.stackFrameTextLength)
    };
}
