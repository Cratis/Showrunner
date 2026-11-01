// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A Secret and the key within it that was found to exist.
/// </summary>
/// <param name="SecretName">The name of the Secret.</param>
/// <param name="Key">The key within it.</param>
public record ResolvedSecretRef(string SecretName, string Key);
