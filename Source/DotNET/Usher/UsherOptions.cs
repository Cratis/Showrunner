// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher;

/// <summary>
/// Represents how an application configures Usher.
/// </summary>
public class UsherOptions
{
    /// <summary>
    /// Gets or sets the name of the application, as the desk sees it on every report. A desk serves several
    /// applications and tells them apart by it. Required.
    /// </summary>
    public ApplicationName ApplicationName { get; set; } = ApplicationName.NotSet;

    /// <summary>
    /// Gets or sets the names of the event stores of the support desks the application talks to. Defaults to
    /// <c>Direct</c>.
    /// </summary>
    public IEnumerable<EventStoreName> DeskStores { get; set; } = ["Direct"];

    /// <summary>
    /// Gets or sets how often Usher looks for tenant namespaces it does not serve yet and for grants of access whose end
    /// has passed. A grant lapses at most this long after its end. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);
}
