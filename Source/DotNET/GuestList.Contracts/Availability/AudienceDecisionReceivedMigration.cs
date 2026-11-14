// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events.Migrations;
using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Migrates <see cref="AudienceDecisionReceived"/> between generation 1 and generation 2.
/// </summary>
/// <remarks>
/// Upcasting names no system, so a receipt that was published before systems were named is for the product-wide
/// <see cref="AudienceLevel.System"/> holder. Downcasting a receipt for one named system turns it into a tenant receipt with
/// no tenant, which pairs with no decision of a generation 1 operator.
/// </remarks>
public class AudienceDecisionReceivedMigration : EventTypeMigration<AudienceDecisionReceived, AudienceDecisionReceivedV1>
{
    /// <inheritdoc/>
    public override void Upcast(IEventMigrationBuilder<AudienceDecisionReceived, AudienceDecisionReceivedV1> builder) =>
        builder.Properties(properties => properties
            .DefaultValue(received => received.ClientId, SystemClientId.NotSet)
            .MapValues(received => received.Level, previous => previous.Level, map => map
                .Map(AudienceLevelV1.Tenant, AudienceLevel.Tenant)
                .Map(AudienceLevelV1.User, AudienceLevel.User)
                .Map(AudienceLevelV1.System, AudienceLevel.System)));

    /// <inheritdoc/>
    public override void Downcast(IEventMigrationBuilder<AudienceDecisionReceivedV1, AudienceDecisionReceived> builder) =>
        builder.Properties(properties => properties
            .MapValues(previous => previous.Level, received => received.Level, map => map
                .Map(AudienceLevel.Tenant, AudienceLevelV1.Tenant)
                .Map(AudienceLevel.User, AudienceLevelV1.User)
                .Map(AudienceLevel.System, AudienceLevelV1.System)
                .Map(AudienceLevel.NamedSystem, AudienceLevelV1.Tenant)));
}
