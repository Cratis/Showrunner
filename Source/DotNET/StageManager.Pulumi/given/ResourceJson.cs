// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// Reads the inputs of a recorded resource as JSON, so a specification navigates a CronJob or a Role the way a manifest reads.
/// </summary>
public static class ResourceJson
{
    /// <summary>
    /// Gets the inputs of a resource as a JSON object.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>The inputs.</returns>
    public static JsonNode Of(RecordedResource resource) => JsonSerializer.SerializeToNode(resource.Inputs)!;

    /// <summary>
    /// Gets the environment variables of the only container of a CronJob, by name, with a secret reference rendered as
    /// <c>secret:NAME/KEY</c> and a field reference as <c>field:PATH</c>.
    /// </summary>
    /// <param name="cronJob">The CronJob.</param>
    /// <returns>The variables.</returns>
    public static Dictionary<string, string> Environment(RecordedResource cronJob)
    {
        var container = Container(cronJob);
        return container["env"]!.AsArray().ToDictionary(
            variable => (string)variable!["name"]!,
            variable => variable!["value"] is { } value
                ? (string)value!
                : variable["valueFrom"]!["secretKeyRef"] is { } secret
                    ? $"secret:{(string)secret["name"]!}/{(string)secret["key"]!}"
                    : $"field:{(string)variable["valueFrom"]!["fieldRef"]!["fieldPath"]!}",
            StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the only container of a CronJob.
    /// </summary>
    /// <param name="cronJob">The CronJob.</param>
    /// <returns>The container.</returns>
    public static JsonNode Container(RecordedResource cronJob) => Of(cronJob)["spec"]!["jobTemplate"]!["spec"]!["template"]!["spec"]!["containers"]![0]!;

    /// <summary>
    /// Gets the rules of a Role or ClusterRole as <c>group/resource:verb,verb</c>, with <c>[name]</c> after the resource when the
    /// rule names it.
    /// </summary>
    /// <param name="role">The Role or ClusterRole.</param>
    /// <returns>The rules.</returns>
    public static List<string> Rules(RecordedResource role) => [.. Of(role)["rules"]!.AsArray().Select(rule =>
    {
        var names = rule!["resourceNames"] is JsonArray resourceNames ? "[" + string.Join(',', resourceNames.Select(name => (string)name!)) + "]" : string.Empty;
        return $"{string.Join(',', rule["apiGroups"]!.AsArray().Select(group => (string)group!))}/{string.Join(',', rule["resources"]!.AsArray().Select(resource => (string)resource!))}{names}:{string.Join(',', rule["verbs"]!.AsArray().Select(verb => (string)verb!))}";
    })];
}
#endif
