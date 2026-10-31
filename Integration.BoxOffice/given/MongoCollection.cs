// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Integration.given;

/// <summary>
/// Specifications that share the MongoDB container.
/// </summary>
[CollectionDefinition(Name)]
public class MongoCollection : ICollectionFixture<MongoInfrastructure>
{
    /// <summary>
    /// The name of the collection.
    /// </summary>
    public const string Name = "MongoDB";
}
