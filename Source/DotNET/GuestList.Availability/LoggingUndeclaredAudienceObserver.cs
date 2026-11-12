// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Cratis.Choreography.Availability.Audiences;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Reports a grant for an audience the application does not declare by logging a warning, once for each subject and grant
/// for as long as the process runs. The grant is kept and never matches a gate; the <c>UndeclaredGrant</c> query lists them
/// for an operator.
/// </summary>
/// <param name="logger">The logger.</param>
public class LoggingUndeclaredAudienceObserver(ILogger<LoggingUndeclaredAudienceObserver> logger) : IUndeclaredAudienceObserver
{
    readonly ConcurrentDictionary<(string? Tenant, string? User, AudienceLevel Level, string Audience), byte> _reported = [];

    /// <inheritdoc/>
    public void Observe(AudienceSubject subject, IReadOnlyCollection<AudienceGrant> undeclared)
    {
        foreach (var grant in undeclared)
        {
            if (_reported.TryAdd((subject.Tenant, subject.User, grant.Level, grant.Audience.Value), 0))
            {
                logger.GrantedButNotDeclared(grant.Audience.Value, grant.Level, subject.Tenant, subject.User);
            }
        }
    }
}
