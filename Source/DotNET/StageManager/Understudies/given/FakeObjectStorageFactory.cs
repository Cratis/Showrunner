// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A factory that hands out in-memory buckets by name, and remembers the settings it was asked for.
/// </summary>
public class FakeObjectStorageFactory : IObjectStorageFactory
{
    public Dictionary<string, InMemoryObjectStorage> Buckets { get; } = new(StringComparer.Ordinal);

    public List<ObjectStorageSettings> Asked { get; } = [];

    public InMemoryObjectStorage Bucket(string name)
    {
        if (!Buckets.TryGetValue(name, out var bucket))
        {
            Buckets[name] = bucket = new();
        }

        return bucket;
    }

    public IObjectStorage Create(ObjectStorageSettings settings)
    {
        Asked.Add(settings);
        return Bucket(settings.Bucket);
    }
}
#endif
