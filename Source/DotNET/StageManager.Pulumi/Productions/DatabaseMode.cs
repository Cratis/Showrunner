// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Who runs the database of a Production.
/// </summary>
public enum DatabaseMode
{
    /// <summary>
    /// The database is run by something else and reached through a connection string.
    /// </summary>
    External = 0,

    /// <summary>
    /// Stage Manager runs the database (a Percona MongoDB replica set).
    /// </summary>
    Managed = 1
}
