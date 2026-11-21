// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A typed endpoint check - everything about the request is pinned here; nothing is chosen at execution time.
/// </summary>
/// <param name="Name">The check's name, unique on its instance.</param>
/// <param name="Resource">The operational resource it watches.</param>
/// <param name="Method">The method - <see cref="WatchdogHttpMethod.Get"/> or <see cref="WatchdogHttpMethod.Head"/>.</param>
/// <param name="BaseUrl">The absolute base URL.</param>
/// <param name="Path">The path, starting with <c>/</c>, appended to <see cref="BaseUrl"/>.</param>
/// <param name="ExpectedStatusFrom">The lowest status code that passes.</param>
/// <param name="ExpectedStatusTo">The highest status code that passes.</param>
/// <param name="Timeout">How long one request may take.</param>
/// <param name="Interval">How often the check runs.</param>
/// <param name="Credential">The credential reference sent as a bearer token - empty for none; resolved from
/// <see cref="WatchdogConfigurationSnapshot.Secrets"/>.</param>
public record EndpointCheckDefinition(
    string Name,
    Guid Resource,
    WatchdogHttpMethod Method,
    string BaseUrl,
    string Path,
    int ExpectedStatusFrom,
    int ExpectedStatusTo,
    TimeSpan Timeout,
    TimeSpan Interval,
    string Credential);
