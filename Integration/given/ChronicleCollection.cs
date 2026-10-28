// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// The specifications that share one Chronicle kernel.
/// </summary>
[CollectionDefinition(Name)]
public class ChronicleCollection : ICollectionFixture<ChronicleInfrastructure>
{
    /// <summary>
    /// The name of the collection.
    /// </summary>
    public const string Name = "Chronicle";
}
