// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Thrown when the volume sweep cannot form a trustworthy view of the cluster and the cloud.
/// </summary>
/// <param name="message">Why it cannot run.</param>
public sealed class VolumeSweepCannotRunException(string message) : Exception(message);
