// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// Dumps one database to a compressed archive file. It is the raw database dump: never Chronicle's event export, which decrypts personal data.
/// </summary>
public interface IDumpTool
{
    /// <summary>
    /// Dumps a database.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="archivePath">The file to write the archive to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the archive is written.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the dump fails.</exception>
    Task DumpAsync(string database, string archivePath, CancellationToken cancellationToken);
}
