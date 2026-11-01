// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Where the Chronicle connection string is: one key of an existing Secret.
/// </summary>
/// <param name="SecretName">The name of the Secret.</param>
/// <param name="Key">The key within the Secret.</param>
public record ChronicleConnectionRef(string SecretName, string Key)
{
    /// <summary>
    /// The key a connection Secret uses when the Performance names none.
    /// </summary>
    public const string DefaultKey = "connectionString";

    /// <summary>
    /// The name of the file the connection string gets inside a mounted folder.
    /// </summary>
    public const string FileName = "Cratis__Chronicle__ConnectionString";
}
