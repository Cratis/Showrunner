// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// An object in <see cref="IObjectStorage"/>.
/// </summary>
/// <param name="Key">The key.</param>
/// <param name="Size">The size in bytes.</param>
/// <param name="LastModified">When it was last written.</param>
public record StoredObject(string Key, long Size, DateTimeOffset LastModified);
