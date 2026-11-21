// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The storage engine of a Production's database.
/// </summary>
public enum DatabaseKind
{
    /// <summary>
    /// MongoDB, the default.
    /// </summary>
    MongoDB = 0,

    /// <summary>
    /// PostgreSQL. External only.
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// SQL Server. External only.
    /// </summary>
    SqlServer = 2
}
