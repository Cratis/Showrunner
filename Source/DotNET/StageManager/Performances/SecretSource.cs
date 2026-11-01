// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// One Secret that contributes to a mounted folder.
/// </summary>
/// <param name="SecretName">The name of the Secret.</param>
/// <param name="Items">The keys to take and the file each becomes, or null to take every key under its own name.</param>
public record SecretSource(string SecretName, IReadOnlyDictionary<string, string>? Items = null);
