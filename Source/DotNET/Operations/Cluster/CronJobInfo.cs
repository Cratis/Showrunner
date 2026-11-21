// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A CronJob.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Created">When it was created.</param>
/// <param name="LastSuccessful">When one of its Jobs last succeeded, when one has.</param>
public sealed record CronJobInfo(string Name, DateTimeOffset? Created, DateTimeOffset? LastSuccessful);
