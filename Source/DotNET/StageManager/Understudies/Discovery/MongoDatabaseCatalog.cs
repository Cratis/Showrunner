// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MongoDB.Driver;

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// <see cref="IDatabaseCatalog"/> over MongoDB.
/// </summary>
/// <param name="connectionString">The connection string. It carries credentials and is never logged.</param>
public class MongoDatabaseCatalog(string connectionString) : IDatabaseCatalog
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        using var client = new MongoClient(connectionString);
        using var cursor = await client.ListDatabaseNamesAsync(cancellationToken);
        return await cursor.ToListAsync(cancellationToken);
    }
}
