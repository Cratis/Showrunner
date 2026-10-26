// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Tenancy;

/// <summary>
/// Log messages for <see cref="UsherWorker"/>.
/// </summary>
internal static partial class UsherWorkerLogging
{
    [LoggerMessage(LogLevel.Warning, "Usher's background round failed; it runs again after the polling interval")]
    internal static partial void RoundFailed(this ILogger<UsherWorker> logger, Exception exception);

    [LoggerMessage(LogLevel.Warning, "Lapsing grants of access in namespace '{Namespace}' failed; it runs again after the polling interval")]
    internal static partial void LapsingFailed(this ILogger<UsherWorker> logger, Exception exception, EventStoreNamespaceName @namespace);
}
