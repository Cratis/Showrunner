// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// CPU and memory requests and limits of the application container, as Kubernetes quantities.
/// </summary>
public sealed class ApplicationResourcesArgs
{
    /// <summary>
    /// Gets the requests by resource name, for example <c>cpu</c> as <c>250m</c> and <c>memory</c> as <c>512Mi</c>.
    /// </summary>
    public IDictionary<string, Input<string>> Requests { get; init; } = new Dictionary<string, Input<string>>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the limits by resource name, for example <c>cpu</c> as <c>1</c> and <c>memory</c> as <c>1Gi</c>.
    /// </summary>
    public IDictionary<string, Input<string>> Limits { get; init; } = new Dictionary<string, Input<string>>(StringComparer.Ordinal);

    /// <summary>
    /// Builds the spec of the resources.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutMap("requests", Requests.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .PutMap("limits", Limits.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)));
}
