// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The key something signs with now and the public keys of those it signed with before.
/// </summary>
/// <param name="Current">The key in use.</param>
/// <param name="Previous">The previous public keys, newest first.</param>
public record KeyRing(SigningKeyMaterial Current, IReadOnlyList<PublicKeyMaterial> Previous);
