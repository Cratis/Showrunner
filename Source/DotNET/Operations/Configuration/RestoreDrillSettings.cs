// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Where the restore drill runs and how long it may go without succeeding.
/// </summary>
/// <param name="Namespace">The namespace the drill CronJob runs in.</param>
/// <param name="CronJob">The name of the drill CronJob.</param>
/// <param name="StaleAfter">How long it may go without succeeding.</param>
public sealed record RestoreDrillSettings(string Namespace, string CronJob, TimeSpan StaleAfter);
