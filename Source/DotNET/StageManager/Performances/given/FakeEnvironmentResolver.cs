// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// An environment resolver that returns a fixed resolution.
/// </summary>
/// <param name="resolution">The resolution to return.</param>
public class FakeEnvironmentResolver(EnvironmentResolution resolution) : IEnvironmentResolver
{
    public Task<EnvironmentResolution> ResolveAsync(string name, string environmentFile, string? passphrase, CancellationToken cancellationToken) =>
        Task.FromResult(resolution);
}
#endif
