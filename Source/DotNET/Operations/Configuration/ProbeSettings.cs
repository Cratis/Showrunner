// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Probe;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// What the synthetic probe calls and where it reports.
/// </summary>
/// <param name="Targets">The public sites and the requests to make.</param>
/// <param name="RoundGap">The pause before the failing requests are made again.</param>
/// <param name="PingUrl">The Healthchecks ping address of the external probe, or <see langword="null"/> when it does not ping. It is a capability and is never logged.</param>
/// <param name="IssueRepository">The repository whose outage issue the external probe keeps, as <c>owner/name</c>, or <see langword="null"/> when it keeps none.</param>
/// <param name="IssueToken">The token that may write issues. It is never logged.</param>
public sealed record ProbeSettings(IReadOnlyList<ProbeTarget> Targets, TimeSpan RoundGap, Uri? PingUrl, string? IssueRepository, string? IssueToken);
