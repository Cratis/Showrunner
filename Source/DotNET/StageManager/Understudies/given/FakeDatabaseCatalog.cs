// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A database server that holds the databases it is given.
/// </summary>
/// <param name="names">The names.</param>
public class FakeDatabaseCatalog(IEnumerable<string> names) : IDatabaseCatalog
{
    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<string>>([.. names]);
}
#endif
