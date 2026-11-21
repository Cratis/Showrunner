// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

internal static partial class DirectLinkLogMessages
{
    [LoggerMessage(LogLevel.Information, "direct link: {Status}")]
    internal static partial void Status(this ILogger<DirectLink> logger, string status);

    [LoggerMessage(LogLevel.Warning, "direct link: {Status}")]
    internal static partial void Degraded(this ILogger<DirectLink> logger, string status);

    [LoggerMessage(LogLevel.Information, "direct link: applied configuration revision {Revision}")]
    internal static partial void Applied(this ILogger<DirectLink> logger, long revision);

    [LoggerMessage(LogLevel.Warning, "direct link: could not save its state: {Error}")]
    internal static partial void StateNotSaved(this ILogger<DirectLink> logger, string error);
}
