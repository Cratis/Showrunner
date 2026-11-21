// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A typed remedy - the request is pinned, and so are the rules deciding whether it may run.
/// </summary>
/// <param name="Name">The remedy's name, unique on its instance.</param>
/// <param name="Resource">The operational resource it acts on.</param>
/// <param name="Method">The method - <see cref="WatchdogHttpMethod.Post"/> or <see cref="WatchdogHttpMethod.Put"/>.</param>
/// <param name="BaseUrl">The absolute base URL.</param>
/// <param name="Path">The path, starting with <c>/</c>.</param>
/// <param name="Parameters">The fixed body parameters.</param>
/// <param name="Timeout">How long the request may take.</param>
/// <param name="Credential">The credential reference sent as a bearer token.</param>
/// <param name="Verification">The check that proves the remedy worked - and whose failure the remedy answers.</param>
/// <param name="OnlineMode">How it may run while Direct is reachable.</param>
/// <param name="OfflineMode">How it may run while Direct is not.</param>
/// <param name="MaxAttempts">How many attempts <see cref="OfflineRemedyPolicy.AttemptWindow"/> may hold.</param>
/// <param name="Cooldown">How long after an attempt the next may start.</param>
/// <param name="EvidenceFreshness">How old the failing observation may be.</param>
public record RemedyDefinition(
    string Name,
    Guid Resource,
    WatchdogHttpMethod Method,
    string BaseUrl,
    string Path,
    IReadOnlyList<RemedyParameterDefinition> Parameters,
    TimeSpan Timeout,
    string Credential,
    string Verification,
    RemedyPolicyMode OnlineMode,
    RemedyPolicyMode OfflineMode,
    int MaxAttempts,
    TimeSpan Cooldown,
    TimeSpan EvidenceFreshness);
