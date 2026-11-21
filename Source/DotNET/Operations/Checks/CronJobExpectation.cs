// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// A CronJob that must keep succeeding: a database backup, another watchdog, a drill.
/// </summary>
/// <param name="Namespace">The namespace of the CronJob.</param>
/// <param name="Name">The name of the CronJob.</param>
/// <param name="MaximumAge">How long it may go without a successful run. A CronJob that has never succeeded is measured from its creation, so a new deploy is not flagged at once.</param>
/// <param name="Optional">Whether a missing CronJob is fine. When it is not, a missing CronJob is a finding.</param>
public sealed record CronJobExpectation(string Namespace, string Name, TimeSpan MaximumAge, bool Optional = false);
