// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Testing;

namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// What a Pulumi program declared when it ran against <see cref="RecordingMocks"/>, and the means to read it back as the
/// operator would: the spec of a custom resource deserialized into the operator's own spec type.
/// </summary>
/// <param name="resources">The resources the program registered.</param>
public sealed class DeclaredResources(IReadOnlyList<RecordedResource> resources)
{
    const string CustomResourcePrefix = $"kubernetes:{ShowrunnerApi.ApiVersion}:";
    const string SecretType = "kubernetes:core/v1:Secret";
    const string ConfigMapType = "kubernetes:core/v1:ConfigMap";

    /// <summary>
    /// Gets the Secrets the program declared.
    /// </summary>
    public IReadOnlyList<RecordedResource> Secrets => [.. resources.Where(resource => resource.Type == SecretType)];

    /// <summary>
    /// Gets the ConfigMaps the program declared.
    /// </summary>
    public IReadOnlyList<RecordedResource> ConfigMaps => [.. resources.Where(resource => resource.Type == ConfigMapType)];

    /// <summary>
    /// Runs a Pulumi program against mocks, with no engine and no cluster.
    /// </summary>
    /// <param name="program">The program, which declares components.</param>
    /// <returns>What it declared.</returns>
    public static async Task<DeclaredResources> Run(Action program)
    {
        var mocks = new RecordingMocks();
        await Deployment.TestAsync(
            mocks,
            new TestOptions { IsPreview = false },
            () =>
            {
                program();
                return Task.FromResult(new Dictionary<string, object?>());
            });
        return new DeclaredResources(mocks.Resources);
    }

    /// <summary>
    /// Gets the custom resource of a kind.
    /// </summary>
    /// <param name="kind">The kind: <c>Production</c>, <c>Performance</c> or <c>Understudy</c>.</param>
    /// <returns>The only custom resource of that kind.</returns>
    public RecordedResource Custom(string kind) => resources.Single(resource => resource.Type == CustomResourcePrefix + kind);

    /// <summary>
    /// Gets the Secret with a Kubernetes name.
    /// </summary>
    /// <param name="name">The name of the Secret.</param>
    /// <returns>The Secret.</returns>
    public RecordedResource Secret(string name) => Secrets.Single(secret => secret.KubernetesName == name);

    /// <summary>
    /// Reads the spec of a custom resource as the operator does: strictly, so a field the operator does not know fails.
    /// </summary>
    /// <typeparam name="TSpec">The operator's spec type.</typeparam>
    /// <param name="kind">The kind of the custom resource.</param>
    /// <returns>The spec.</returns>
    public TSpec SpecOf<TSpec>(string kind) => OperatorJson.Deserialize<TSpec>(Custom(kind).Inputs["spec"]);

    /// <summary>
    /// Reads a value of a nested object of a Secret's string data.
    /// </summary>
    /// <param name="secretName">The name of the Secret.</param>
    /// <param name="key">The key in the Secret.</param>
    /// <returns>The value as it was handed to the engine.</returns>
    public object StringData(string secretName, string key) => ((ImmutableDictionary<string, object>)Secret(secretName).Inputs["stringData"])[key];
}
#endif
