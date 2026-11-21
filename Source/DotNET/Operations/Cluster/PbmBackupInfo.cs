// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A Percona Backup for MongoDB backup resource.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="State">The state, for example <c>ready</c> or <c>error</c>.</param>
/// <param name="Created">When it was created.</param>
/// <param name="Completed">When it completed successfully.</param>
/// <param name="Error">The error it recorded.</param>
public sealed record PbmBackupInfo(string Name, string? State, DateTimeOffset? Created, DateTimeOffset? Completed, string? Error);
