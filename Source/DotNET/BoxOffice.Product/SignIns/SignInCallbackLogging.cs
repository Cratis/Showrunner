// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

internal static partial class SignInCallbackLogging
{
    [LoggerMessage(LogLevel.Warning, "A call to the sign-in callback was refused: it is not AuthProxy's, or its secret is wrong")]
    internal static partial void NotFromAuthProxy(this ILogger<SignInCalls> logger);

    [LoggerMessage(LogLevel.Warning, "The sign-in callback was refused: {Reason}")]
    internal static partial void BadCallback(this ILogger<SignInCalls> logger, string reason);

    [LoggerMessage(LogLevel.Debug, "The sign-in callback named no tenant, so there is no tenant to report activity in")]
    internal static partial void NoTenant(this ILogger<SignInCalls> logger);
}
