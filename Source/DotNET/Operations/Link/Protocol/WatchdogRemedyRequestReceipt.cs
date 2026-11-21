// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Direct recorded a remedy request; its gate is evaluated asynchronously.
/// </summary>
/// <param name="Request">The request.</param>
/// <param name="Recorded">Whether this delivery recorded it - false when it was already recorded.</param>
public record WatchdogRemedyRequestReceipt(Guid Request, bool Recorded);
