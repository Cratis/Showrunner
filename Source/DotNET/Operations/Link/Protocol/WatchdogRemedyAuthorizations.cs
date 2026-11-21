// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The remedy requests an instance may execute now.
/// </summary>
/// <param name="Authorized">The authorizations.</param>
public record WatchdogRemedyAuthorizations(IReadOnlyList<WatchdogRemedyAuthorization> Authorized);
