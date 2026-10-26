// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Log messages for <see cref="OnBehalfForwards"/>. They never carry a token or a header value.
/// </summary>
internal static partial class OnBehalfForwardsLogging
{
    [LoggerMessage(LogLevel.Warning, "Refused a request forwarded on behalf of a user: its assertion was not accepted ({Rejection})")]
    internal static partial void ForwardRefused(this ILogger<OnBehalfForwards> logger, OnBehalfTokenRejection rejection);

    [LoggerMessage(LogLevel.Warning, "Refused a request forwarded on behalf of a user: its tenant is not the tenant of the session")]
    internal static partial void ForwardInAnotherTenant(this ILogger<OnBehalfForwards> logger);
}
