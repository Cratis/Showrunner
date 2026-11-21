// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// An active scrape target as Prometheus reports it.
/// </summary>
/// <param name="Job">The scrape job.</param>
/// <param name="Instance">The instance.</param>
/// <param name="Health">The health: <c>up</c>, <c>down</c> or <c>unknown</c>.</param>
/// <param name="LastError">The last scrape error; empty when there is none.</param>
public sealed record ScrapeTarget(string Job, string Instance, string Health, string LastError);
