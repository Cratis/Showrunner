// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Text.Json;
using System.Text.RegularExpressions;
using Cratis.Showrunner.StageManager.Pulumi.given;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using OperatorPerformance = Cratis.Showrunner.StageManager.Performances.Performance;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;
using OperatorSpecValidator = Cratis.Showrunner.StageManager.Performances.SpecValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

/// <summary>
/// The YAML of the Performance page is the first thing a reader copies, so every key in it has to be a field of the operator's spec.
/// </summary>
public partial class when_reading_the_documented_yaml : Specification
{
    List<(string Name, OperatorPerformance Performance)> _examples;

    [GeneratedRegex("```yaml\\r?\\n(?<yaml>.*?)```", RegexOptions.Singleline, matchTimeoutMilliseconds: 1000)]
    private static partial Regex YamlBlock { get; }

    static string PagePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Documentation", "StageManager", "performance.md")))
        {
            directory = directory.Parent;
        }

        return Path.Combine(directory!.FullName, "Documentation", "StageManager", "performance.md");
    }

    /// <summary>
    /// Converts a YAML node to a value JSON can carry: the plain scalars true, false and numbers keep their type, quoted ones stay text, as in Kubernetes.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The value.</returns>
    static object? ToValue(YamlNode node) => node switch
    {
        YamlMappingNode mapping => mapping.Children.ToDictionary(pair => ((YamlScalarNode)pair.Key).Value!, pair => ToValue(pair.Value), StringComparer.Ordinal),
        YamlSequenceNode sequence => sequence.Children.Select(ToValue).ToList(),
        YamlScalarNode { Style: ScalarStyle.Plain } scalar when bool.TryParse(scalar.Value, out var flag) => flag,
        YamlScalarNode { Style: ScalarStyle.Plain } scalar when long.TryParse(scalar.Value, out var number) => number,
        YamlScalarNode scalar => scalar.Value,
        _ => null
    };

    void Because()
    {
        var strict = new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };
        _examples = [];
        foreach (Match block in YamlBlock.Matches(File.ReadAllText(PagePath())))
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(block.Groups["yaml"].Value));
            var document = (Dictionary<string, object?>)ToValue(stream.Documents[0].RootNode)!;
            if (!document.TryGetValue("kind", out var kind))
            {
                // A fragment of a spec: every key still has to be one of the spec.
                _ = JsonSerializer.Deserialize<OperatorPerformanceSpec>(JsonSerializer.Serialize(document), strict);
                continue;
            }

            if ((kind as string) != "Performance")
            {
                continue;
            }

            var metadata = (Dictionary<string, object?>)document["metadata"]!;
            var spec = JsonSerializer.Deserialize<OperatorPerformanceSpec>(JsonSerializer.Serialize(document["spec"]), strict)!;
            _examples.Add(((string)metadata["name"]!, new OperatorPerformance { Metadata = new() { Name = (string)metadata["name"]!, NamespaceProperty = (string)metadata["namespace"]! }, Spec = spec }));
        }
    }

    [Fact] void should_find_the_examples() => _examples.Count.ShouldBeGreaterThanOrEqual(3);
    [Fact] void should_give_the_operator_nothing_to_reject() => _examples.SelectMany(example => OperatorSpecValidator.Validate(example.Performance).Select(problem => $"{example.Name}: {problem}")).ShouldBeEmpty();
}
#endif
