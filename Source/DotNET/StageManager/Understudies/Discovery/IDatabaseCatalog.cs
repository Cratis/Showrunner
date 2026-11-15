// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// Lists the databases that exist on a Production's database server.
/// </summary>
public interface IDatabaseCatalog
{
    /// <summary>
    /// Lists the database names.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The names.</returns>
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);
}
