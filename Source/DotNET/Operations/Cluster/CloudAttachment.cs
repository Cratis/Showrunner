// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A server a volume is attached to.
/// </summary>
/// <param name="ServerId">The server id.</param>
/// <param name="Started">Whether the server is running; a server in maintenance is left for the next run.</param>
/// <param name="Address">The device address the volume has on the server, which detaching needs.</param>
public sealed record CloudAttachment(string ServerId, bool Started, string Address);
