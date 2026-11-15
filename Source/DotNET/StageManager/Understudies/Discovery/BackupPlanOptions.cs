// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// What the rules need besides the topology and the databases that exist.
/// </summary>
/// <param name="Names">Chronicle's naming, with its prefix.</param>
/// <param name="DeclaredDatabases">The declared database patterns, such as <c>{eventStore}+jobs+{namespace}</c> (per namespace) or <c>orleans</c> (per Production).</param>
/// <param name="HasVault">Whether the Production keeps its keys in a Vault. Without one they travel inside the events and nothing separate is taken.</param>
public record BackupPlanOptions(ChronicleDatabaseNames Names, IReadOnlyList<string> DeclaredDatabases, bool HasVault);
