// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What every request after the claim presents: the instance credential and the fencing epoch.
/// </summary>
/// <param name="Credential">The instance credential.</param>
/// <param name="Epoch">The fencing epoch.</param>
public sealed record DirectCredentials(string Credential, long Epoch);
