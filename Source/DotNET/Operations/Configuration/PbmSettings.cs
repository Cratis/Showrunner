// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Configuration;

/// <summary>
/// Where the Percona Backup for MongoDB backups live and how old the newest may be.
/// </summary>
/// <param name="Namespace">The namespace of the backups.</param>
/// <param name="MaximumAge">How old the newest ready backup may be.</param>
public sealed record PbmSettings(string Namespace, TimeSpan MaximumAge);
