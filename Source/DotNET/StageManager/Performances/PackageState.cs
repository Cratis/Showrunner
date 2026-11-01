// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What is known about the ConfigMap that holds a package.
/// </summary>
/// <param name="HasKey">Whether the package key exists, as data or as binary data.</param>
/// <param name="Immutable">Whether the ConfigMap is immutable.</param>
public record PackageState(bool HasKey, bool Immutable);
