// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// A unit of work that holds one of a meter's concurrent slots until it is ended.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Id">The unit of work.</param>
public record ComputeWork(MeterId Meter, ComputeWorkId Id);
