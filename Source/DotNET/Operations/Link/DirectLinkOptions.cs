// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// How the watchdog is linked to Cratis Direct's watchdog protocol. Optional: without it the watchdog only alerts through its
/// channels.
/// </summary>
public sealed record DirectLinkOptions
{
    /// <summary>
    /// Gets the base URL of Direct, for example <c>https://direct.example.com</c>; the protocol routes are appended to it.
    /// </summary>
    public required Uri BaseUrl { get; init; }

    /// <summary>
    /// Gets the operator-facing name of the watchdog instance in Direct.
    /// </summary>
    public required string InstanceName { get; init; }

    /// <summary>
    /// Gets the one-time enrollment token. It is only used until the first claim succeeded; the instance credential that
    /// replaces it is kept in the link's Kubernetes Secret.
    /// </summary>
    public string? EnrollmentToken { get; init; }

    /// <summary>
    /// Gets the Direct operational resource the cluster observations are reported against. When empty, a stable identity is
    /// derived from <see cref="InstanceName"/>.
    /// </summary>
    public Guid Resource { get; init; }

    /// <summary>
    /// Gets the name of the ConfigMap and Secret the link keeps its state in.
    /// </summary>
    public string StateName { get; init; } = "showrunner-watchdog-direct-link";

    /// <summary>
    /// Gets the Kubernetes actions the watchdog is willing and RBAC'd to take for a Direct remedy of the same name.
    /// </summary>
    public IReadOnlyList<RemedyBinding> Remedies { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether a Direct remedy without a local binding may execute its pinned HTTP request. Off by default.
    /// </summary>
    public bool AllowHttpRemedies { get; init; }

    /// <summary>
    /// Gets the most entries the durable outbox keeps; the oldest are evicted beyond it.
    /// </summary>
    public int OutboxLimit { get; init; } = 800;

    /// <summary>
    /// Gets the runtime version reported to Direct.
    /// </summary>
    public string RuntimeVersion { get; init; } = "0.0.0";

    /// <summary>
    /// Gets the resource the observations are reported against.
    /// </summary>
    public Guid EffectiveResource => Resource != Guid.Empty ? Resource : new Guid(SHA256.HashData(Encoding.UTF8.GetBytes($"showrunner-watchdog:{InstanceName}"))[..16]);
}
