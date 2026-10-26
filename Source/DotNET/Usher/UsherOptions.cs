// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture;

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
    /// Gets or sets the version of the application, recorded with errors the backend captures. Defaults to
    /// <see cref="ApplicationVersion.NotSet"/>, which means the informational version of the entry assembly, without
    /// its build metadata (the part after <c>+</c>).
    /// </summary>
    public ApplicationVersion ApplicationVersion { get; set; } = ApplicationVersion.NotSet;

    /// <summary>
    /// Gets or sets how Usher captures the errors of failing commands, so a report of one can be recorded from what the
    /// backend saw rather than from what the client sends.
    /// </summary>
    public ErrorCaptureOptions ErrorCapture { get; set; } = new();

    /// <summary>
    /// Gets or sets how often Usher looks for tenant namespaces it does not serve yet and for grants of access whose end
    /// has passed. A grant lapses at most this long after its end. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);
}
