// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Configuration;

namespace Cratis.Showrunner.Operations.Watchdog;

/// <summary>
/// Parses the command line and runs the command it names.
/// </summary>
public static class WatchdogCommands
{
    /// <summary>
    /// The exit code for a command line that names no command.
    /// </summary>
    public const int UsageExitCode = 2;

    /// <summary>
    /// Gets the usage text.
    /// </summary>
    public static string Usage => "usage: watchdog <cluster|test-alert|runners|agent-harness|volumes|probe|probe-external>";

    /// <summary>
    /// Reads the command from the arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>The command, or <see cref="WatchdogCommand.Unknown"/>.</returns>
    public static WatchdogCommand Parse(IReadOnlyList<string> args) => args.Count == 1
        ? args[0] switch
        {
            "cluster" => WatchdogCommand.Cluster,
            "test-alert" => WatchdogCommand.TestAlert,
            "runners" => WatchdogCommand.Runners,
            "agent-harness" => WatchdogCommand.AgentHarness,
            "volumes" => WatchdogCommand.Volumes,
            "probe" => WatchdogCommand.Probe,
            "probe-external" => WatchdogCommand.ProbeExternal,
            _ => WatchdogCommand.Unknown
        }
        : WatchdogCommand.Unknown;

    /// <summary>
    /// Runs a command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="factory">Puts the run together.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="log">Where the external probe writes its summary and one-line results; the default is the console.</param>
    /// <param name="stepSummaryPath">The file the external probe appends its Markdown summary to, for the job summary of a CI run.</param>
    /// <returns>The process exit code: 0 when the command succeeded, 1 when it failed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The command is <see cref="WatchdogCommand.Unknown"/>.</exception>
    public static async Task<int> Run(WatchdogCommand command, WatchdogFactory factory, CancellationToken cancellationToken, Action<string>? log = null, string? stepSummaryPath = null)
    {
        if (command == WatchdogCommand.ProbeExternal)
        {
            var (runner, targets, roundGap) = factory.CreateProbeRunner(log ?? Console.WriteLine);
            var (exitCode, summary) = await runner.Run(targets, roundGap, cancellationToken);
            if (!string.IsNullOrEmpty(stepSummaryPath))
            {
                await File.AppendAllTextAsync(stepSummaryPath, summary, cancellationToken);
            }

            return exitCode;
        }

        var outcome = command switch
        {
            WatchdogCommand.Cluster => await factory.CreateClusterRun().Run(cancellationToken),
            WatchdogCommand.TestAlert => await factory.CreateTestAlertRun().RunTestAlert(cancellationToken),
            WatchdogCommand.Runners => await factory.CreateJobRun(Checks.RunnerPoolCheck.CheckName).Run(cancellationToken),
            WatchdogCommand.AgentHarness => await factory.CreateJobRun(Checks.AgentHarnessCheck.CheckName).Run(cancellationToken),
            WatchdogCommand.Volumes => await factory.CreateJobRun(Checks.VolumeSweepCheck.CheckName).Run(cancellationToken),
            WatchdogCommand.Probe => await factory.CreateJobRun(Probe.ProbeCheck.CheckName).Run(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Not a runnable command.")
        };
        return outcome.ExitCode;
    }
}
