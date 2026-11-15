// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// <see cref="IDumpTool"/> over <c>mongodump</c>, which the job image carries. The connection string reaches it through a configuration file that only the job can read,
/// never through its arguments, so it does not show in a process list.
/// </summary>
/// <param name="connectionString">The connection string of the MongoDB. It carries credentials and is never logged.</param>
/// <param name="workDirectory">The directory the configuration file is written to for the time of the dump.</param>
public class MongoDumpTool(string connectionString, string workDirectory) : IDumpTool
{
    /// <inheritdoc/>
    public async Task DumpAsync(string database, string archivePath, CancellationToken cancellationToken)
    {
        var configuration = Path.Combine(workDirectory, $"mongodump-{Guid.NewGuid():N}.yaml");
        await File.WriteAllTextAsync(configuration, $"uri: \"{connectionString.Replace("\"", "\\\"", StringComparison.Ordinal)}\"\n", cancellationToken);
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(configuration, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }

        try
        {
            var start = new ProcessStartInfo("mongodump")
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            start.ArgumentList.Add($"--config={configuration}");
            start.ArgumentList.Add($"--db={database}");
            start.ArgumentList.Add($"--archive={archivePath}");
            start.ArgumentList.Add("--gzip");

            using var process = Process.Start(start) ?? throw new InvalidOperationException("mongodump could not be started.");
            var errors = process.StandardError.ReadToEndAsync(cancellationToken);
            _ = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                var output = await errors;
                throw new InvalidOperationException($"mongodump of '{database}' exited with code {process.ExitCode}: {output[^Math.Min(output.Length, 500)..]}");
            }
        }
        finally
        {
            File.Delete(configuration);
        }
    }
}
