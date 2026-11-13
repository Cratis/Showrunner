// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Microsoft.Extensions.Logging;
using Product = Cratis.Showrunner.GuestList.Contracts.Audiences.Product;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Log messages for GuestList's availability part.
/// </summary>
internal static partial class AvailabilityLogging
{
    [LoggerMessage(LogLevel.Information, "Reported what product '{Product}' declares: {Audiences} audience(s) and {Gates} gate(s)")]
    internal static partial void AvailabilityReported(this ILogger<AvailabilityReporter> logger, Product product, int audiences, int gates);

    [LoggerMessage(LogLevel.Debug, "What product '{Product}' declares is what was last reported; nothing to report")]
    internal static partial void AvailabilityUnchanged(this ILogger<AvailabilityReporter> logger, Product product);

    [LoggerMessage(LogLevel.Debug, "Product '{Product}' declares no availability; nothing to report")]
    internal static partial void NothingDeclared(this ILogger<AvailabilityReporter> logger, Product product);

    [LoggerMessage(LogLevel.Warning, "Could not report what the product declares; trying again soon")]
    internal static partial void ReportingFailed(this ILogger<AvailabilityReportingService> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "A subscriber to audience changes failed for level {Level}, key {Key}; the other subscribers were still told")]
    internal static partial void SubscriberFailed(this ILogger<AudienceChangeFeed> logger, Exception exception, AudienceLevel level, string? key);

    [LoggerMessage(LogLevel.Error, "Watching the audiences in namespace {Namespace} failed; the next round watches it again")]
    internal static partial void WatchFailed(this ILogger<AudienceChangeWatcher> logger, Exception exception, string @namespace);

    [LoggerMessage(LogLevel.Error, "A round of watching audience changes failed; the next round tries again")]
    internal static partial void RoundFailed(this ILogger<AudienceChangeWatcher> logger, Exception exception);

    [LoggerMessage(LogLevel.Warning, "The audience {Audience} is granted at level {Level} but the application does not declare it. It matches no gate until it is declared or withdrawn (tenant {Tenant}, user {User})")]
    internal static partial void GrantedButNotDeclared(this ILogger<LoggingUndeclaredAudienceObserver> logger, string audience, AudienceLevel level, string? tenant, string? user);
}
