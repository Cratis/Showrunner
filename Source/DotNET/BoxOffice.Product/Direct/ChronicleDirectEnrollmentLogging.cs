// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

internal static partial class ChronicleDirectEnrollmentLogging
{
    [LoggerMessage(LogLevel.Information, "The Chronicle kernel is enrolled already; the enrollment code was not used")]
    internal static partial void AlreadyEnrolled(this ILogger<ChronicleDirectEnrollment> logger);

    [LoggerMessage(LogLevel.Warning, "The endpoint the enrollment code carries cannot be read, so it cannot be checked against the pinned host or the typed endpoint")]
    internal static partial void EndpointNotReadable(this ILogger<ChronicleDirectEnrollment> logger);

    [LoggerMessage(LogLevel.Warning, "The kernel enrolled with an endpoint that is not on the pinned host")]
    internal static partial void EndpointNotAllowedAfterRedeeming(this ILogger<ChronicleDirectEnrollment> logger);

    [LoggerMessage(LogLevel.Warning, "Chronicle refused to redeem the enrollment code; the kernel does not say why, and the application's principal must be an administrator or listed in Federation:Enrollment:Applications: {ExceptionType}")]
    internal static partial void RedemptionRefused(this ILogger<ChronicleDirectEnrollment> logger, string exceptionType);

    [LoggerMessage(LogLevel.Warning, "Chronicle could not be reached to redeem the enrollment code: {ExceptionType}")]
    internal static partial void RedemptionNotReachable(this ILogger<ChronicleDirectEnrollment> logger, string exceptionType);

    [LoggerMessage(LogLevel.Warning, "The enrollment status of the Chronicle kernel could not be read: {ExceptionType}")]
    internal static partial void StatusNotAvailable(this ILogger<ChronicleDirectEnrollment> logger, string exceptionType);

    [LoggerMessage(LogLevel.Warning, "The Chronicle kernel no longer holds a usable certificate (enrolled: {Enrolled}, expired: {Expired})")]
    internal static partial void EnrollmentLost(this ILogger<ChronicleDirectEnrollment> logger, bool enrolled, bool expired);
}
