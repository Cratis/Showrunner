// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events.Migrations;

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Migrates <see cref="AudienceWithdrawn"/> between generation 1 and generation 2.
/// </summary>
/// <remarks>
/// Upcasting names no system, so a withdrawal that was published before systems were named stays the product-wide
/// <see cref="AudienceLevel.System"/> withdrawal. Downcasting a withdrawal from one named system turns it into a tenant
/// withdrawal with no tenant, which a generation 1 reader ignores, so it never removes the product-wide grant.
/// </remarks>
public class AudienceWithdrawnMigration : EventTypeMigration<AudienceWithdrawn, AudienceWithdrawnV1>
{
    /// <inheritdoc/>
    public override void Upcast(IEventMigrationBuilder<AudienceWithdrawn, AudienceWithdrawnV1> builder) =>
        builder.Properties(properties => properties
            .DefaultValue(withdrawn => withdrawn.ClientId, SystemClientId.NotSet)
            .DefaultValue(withdrawn => withdrawn.Kind, SystemKind.None)
            .MapValues(withdrawn => withdrawn.Level, previous => previous.Level, map => map
                .Map(AudienceLevelV1.Tenant, AudienceLevel.Tenant)
                .Map(AudienceLevelV1.User, AudienceLevel.User)
                .Map(AudienceLevelV1.System, AudienceLevel.System)));

    /// <inheritdoc/>
    public override void Downcast(IEventMigrationBuilder<AudienceWithdrawnV1, AudienceWithdrawn> builder) =>
        builder.Properties(properties => properties
            .MapValues(previous => previous.Level, withdrawn => withdrawn.Level, map => map
                .Map(AudienceLevel.Tenant, AudienceLevelV1.Tenant)
                .Map(AudienceLevel.User, AudienceLevelV1.User)
                .Map(AudienceLevel.System, AudienceLevelV1.System)
                .Map(AudienceLevel.NamedSystem, AudienceLevelV1.Tenant)));
}
