// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

/// <summary>
/// A product's own event: a session ended, which used compute. Box Office adds no event beside it.
/// </summary>
/// <param name="Organization">The organization of the tenant.</param>
/// <param name="Started">When the session started.</param>
/// <param name="Minutes">How many minutes of compute it used.</param>
[EventType]
public record SessionEnded(OrganizationId Organization, DateTimeOffset Started, long Minutes);
#endif
