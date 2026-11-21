// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What an instance does while Direct is unreachable.
/// </summary>
/// <param name="ObserveAndQueue">Whether checks keep running and observations are queued - always true today.</param>
/// <param name="RemediesPermitted">Whether any remedy may run offline at all.</param>
public record OfflinePolicy(bool ObserveAndQueue, bool RemediesPermitted);
