// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A named MongoDB sink bound to an application.
/// </summary>
/// <param name="Name">The name of the sink, as the Production configures it.</param>
/// <param name="Connection">The Secret key holding its connection string.</param>
public record MongoDBSinkBinding(string Name, ResolvedSecretRef Connection)
{
    /// <summary>
    /// Gets the Cratis Arc setting that names the file of the connection string.
    /// </summary>
    public string Setting => $"Cratis__MongoDB__Sinks__{Name}__ServerFile";

    /// <summary>
    /// Gets the path of the file the connection string is mounted as.
    /// </summary>
    public string File => $"{ApplicationReadSide.SinksDirectory}/{Name}";
}
