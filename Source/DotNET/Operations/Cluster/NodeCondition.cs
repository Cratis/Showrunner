// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// A condition of a node.
/// </summary>
/// <param name="Type">The type, for example <c>Ready</c> or <c>DiskPressure</c>.</param>
/// <param name="Status">The status: <c>True</c>, <c>False</c> or <c>Unknown</c>.</param>
/// <param name="Reason">The reason the node gives.</param>
public sealed record NodeCondition(string Type, string Status, string? Reason = null);
