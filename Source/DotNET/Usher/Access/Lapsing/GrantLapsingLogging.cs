// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Access;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Access.Lapsing;

/// <summary>
/// Log messages for <see cref="GrantLapsing"/>.
/// </summary>
internal static partial class GrantLapsingLogging
{
    [LoggerMessage(LogLevel.Information, "The grant of access on issue '{IssueId}' in namespace '{Namespace}' lapsed at {ValidUntil}")]
    internal static partial void GrantLapsed(this ILogger<GrantLapsing> logger, IssueId issueId, ValidUntil validUntil, EventStoreNamespaceName @namespace);

    [LoggerMessage(LogLevel.Debug, "The grant of access on issue '{IssueId}' in namespace '{Namespace}' changed while lapsing it; it is looked at again in the next round")]
    internal static partial void GrantChangedWhileLapsing(this ILogger<GrantLapsing> logger, IssueId issueId, EventStoreNamespaceName @namespace);
}
