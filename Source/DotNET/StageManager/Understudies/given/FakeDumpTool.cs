// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A dump tool that writes a small archive per database, and fails for the databases it is told to.
/// </summary>
public class FakeDumpTool : IDumpTool
{
    public HashSet<string> Failing { get; } = new(StringComparer.Ordinal);

    public List<string> Dumped { get; } = [];

    public static byte[] ContentOf(string database) => Encoding.UTF8.GetBytes($"archive of {database}");

    public async Task DumpAsync(string database, string archivePath, CancellationToken cancellationToken)
    {
        if (Failing.Contains(database))
        {
            throw new InvalidOperationException($"mongodump of '{database}' exited with code 1");
        }

        Dumped.Add(database);
        await File.WriteAllBytesAsync(archivePath, ContentOf(database), cancellationToken);
    }
}
#endif
