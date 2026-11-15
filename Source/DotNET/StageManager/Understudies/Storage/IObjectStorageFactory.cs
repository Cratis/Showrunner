// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Creates the <see cref="IObjectStorage"/> a set of <see cref="ObjectStorageSettings"/> describes.
/// </summary>
public interface IObjectStorageFactory
{
    /// <summary>
    /// Creates the storage. The caller disposes it when it is <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The storage.</returns>
    IObjectStorage Create(ObjectStorageSettings settings);
}
