// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What a starting runtime tells Direct when it claims its name. The credential travels in <c>Authorization: Bearer</c>,
/// never in the body.
/// </summary>
/// <param name="RuntimeVersion">The runtime's version.</param>
/// <param name="Session">A value the process chose and keeps - how Direct tells a restart of the same process from a clone.</param>
/// <param name="Variant">The image the runtime runs as.</param>
public record WatchdogClaimRequest(string RuntimeVersion, string Session, WatchdogVariant Variant);
