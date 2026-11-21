// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Diagnostics;
using System.Text.Json;

namespace Cratis.Showrunner.StageManager.Pulumi.Operators.for_StageManagerOperator;

/// <summary>
/// Runs what the component renders through <c>helm template</c> of the chart in this repository, so a value key that drifted from the
/// chart's <c>values.yaml</c> shows up as an image, a namespace or a setting that did not arrive. It does nothing when helm is not on the PATH.
/// </summary>
public class when_rendering_the_values_through_the_chart : Specification
{
    string _manifest;
    bool _helm;
    bool _rendered;

    async Task Because()
    {
        var chart = FindChart();
        if (chart is null || !await Helm("version", "--short").ContinueWith(task => task.IsCompletedSuccessfully && task.Result.Exit == 0))
        {
            return;
        }

        _helm = true;
        var values = Path.Combine(Path.GetTempPath(), $"stage-manager-values-{Guid.NewGuid():N}.json");
        try
        {
            var args = new StageManagerOperatorArgs
            {
                Version = "9.8.7",
                ManagedNamespaces = ["apps", "databases"],
                OperatorImageRepository = "mirror.example.com/showrunner/operator",
                UnderstudyImageRepository = "mirror.example.com/showrunner/understudy",
                Values = new Dictionary<string, object> { ["metrics"] = new Dictionary<string, object> { ["port"] = 9191 } }
            };

            // JSON is valid YAML, which keeps this free of a YAML writer.
            await File.WriteAllTextAsync(values, JsonSerializer.Serialize(StageManagerOperatorValues.Render(args)));
            var result = await Helm("template", "stage-manager", chart, "--namespace", "stage-manager", "--values", values, "--include-crds=false");
            _rendered = result.Exit == 0;
            _manifest = result.Output;
        }
        finally
        {
            File.Delete(values);
        }
    }

    [Fact] void should_render_when_helm_is_available() => (!_helm || _rendered).ShouldBeTrue();
    [Fact] void should_run_the_operator_image_of_the_version() => (!_helm || _manifest.Contains("image: \"mirror.example.com/showrunner/operator:9.8.7\"", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_hand_the_understudy_image_of_the_version_to_the_operator() => (!_helm || _manifest.Contains("\"mirror.example.com/showrunner/understudy:9.8.7\"", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_grant_a_role_in_each_managed_namespace() => (!_helm || (_manifest.Contains("namespace: apps", StringComparison.Ordinal) && _manifest.Contains("namespace: databases", StringComparison.Ordinal))).ShouldBeTrue();
    [Fact] void should_serve_metrics_where_the_passthrough_said() => (!_helm || _manifest.Contains("9191", StringComparison.Ordinal)).ShouldBeTrue();

    static string? FindChart()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var chart = Path.Combine(directory.FullName, "Source", "Helm", "showrunner-operator");
            if (File.Exists(Path.Combine(chart, "Chart.yaml")))
            {
                return chart;
            }
        }

        return null;
    }

    static async Task<(int Exit, string Output)> Helm(params string[] arguments)
    {
        var start = new ProcessStartInfo("helm") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await output + await error);
    }
}
#endif
