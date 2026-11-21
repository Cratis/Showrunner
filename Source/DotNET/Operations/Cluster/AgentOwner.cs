// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// An owner reference of a Pod.
/// </summary>
/// <param name="Kind">The kind of the owner.</param>
/// <param name="ApiVersion">The API version of the owner.</param>
/// <param name="Name">The name of the owner.</param>
/// <param name="Uid">The UID of the owner.</param>
/// <param name="Controller">Whether the owner is the controller of the Pod.</param>
public sealed record AgentOwner(string Kind, string ApiVersion, string Name, string Uid, bool Controller);
