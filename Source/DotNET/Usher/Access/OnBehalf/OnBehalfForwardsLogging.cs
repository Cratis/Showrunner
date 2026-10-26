// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Log messages for <see cref="OnBehalfForwards"/>. They never name the people involved or carry a token.
/// </summary>
internal static partial class OnBehalfForwardsLogging
{
    [LoggerMessage(LogLevel.Warning, "Refused a request forwarded on behalf of a user: Arc did not accept it as an on-behalf forward")]
    internal static partial void ForwardNotAccepted(this ILogger<OnBehalfForwards> logger);

    [LoggerMessage(LogLevel.Warning, "Refused a request forwarded on behalf of a user: its assertion does not name a session of the users it was forwarded as")]
    internal static partial void ForwardWithoutSession(this ILogger<OnBehalfForwards> logger);

    [LoggerMessage(LogLevel.Warning, "Refused a request forwarded on behalf of a user: its tenant is not the tenant of the session")]
    internal static partial void ForwardInAnotherTenant(this ILogger<OnBehalfForwards> logger);
}
