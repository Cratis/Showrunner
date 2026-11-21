// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Checks;

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// How the volume sweeper reaches the cloud account and what it may do.
/// </summary>
/// <param name="Username">The UpCloud API user.</param>
/// <param name="Password">The UpCloud API password. It is never logged.</param>
/// <param name="Options">The sweep options.</param>
public sealed record VolumeSweeperSettings(string Username, string Password, VolumeSweepOptions Options);
