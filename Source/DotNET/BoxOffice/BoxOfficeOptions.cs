// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// The options of Box Office.
/// </summary>
public class BoxOfficeOptions
{
    /// <summary>
    /// Gets or sets the names of the event stores of the operators whose decisions the product honors. Defaults to
    /// <c>Direct</c>.
    /// </summary>
    public IEnumerable<EventStoreName> OperatorStores { get; set; } = ["Direct"];

    /// <summary>
    /// Gets or sets how often Box Office looks for tenant namespaces it does not serve yet. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets whether commands, queries and reactors of a module the tenant does not have are refused. On by default;
    /// turning it off is the operational escape hatch for a deployment whose tenants have not been given their entitlements
    /// yet, and nothing else.
    /// </summary>
    public bool EnforceModules { get; set; } = true;

    /// <summary>
    /// Gets or sets how long a unit of work that was admitted against a concurrency limit counts as running when nobody
    /// ends it, so work that crashed does not hold its slot forever. Defaults to 12 hours, and must be more than zero and
    /// at most seven days.
    /// </summary>
    public TimeSpan MaximumWorkDuration { get; set; } = TimeSpan.FromHours(12);

    /// <summary>
    /// Gets or sets where the operator's customer portal is, which "Buy", "Try" and "Change plan" open. Without it the
    /// product has no customer portal, and the frontend does not offer to open one.
    /// </summary>
    public Uri? CustomerPortalUrl { get; set; }

    /// <summary>
    /// Gets or sets the key shared with the customer portal, which every link to it is signed with. Keep it in a secret
    /// store, never in source. Required when <see cref="CustomerPortalUrl"/> is set.
    /// </summary>
    public string? CustomerPortalSigningKey { get; set; }

    /// <summary>
    /// Gets or sets how long a link to the customer portal works. Defaults to fifteen minutes.
    /// </summary>
    public TimeSpan CustomerPortalLinkLifetime { get; set; } = TimeSpan.FromMinutes(15);
}
