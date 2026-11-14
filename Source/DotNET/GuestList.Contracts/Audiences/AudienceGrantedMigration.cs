// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events.Migrations;

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Migrates <see cref="AudienceGranted"/> between generation 1 and generation 2.
/// </summary>
/// <remarks>
/// Upcasting names no system, so a grant that was published before systems were named stays the product-wide
/// <see cref="AudienceLevel.System"/> holder. Downcasting a grant to one named system turns it into a tenant grant with no
/// tenant, which a generation 1 reader treats as malformed and ignores: a reader that does not know named systems never
/// widens a named grant into a product-wide one.
/// </remarks>
public class AudienceGrantedMigration : EventTypeMigration<AudienceGranted, AudienceGrantedV1>
{
    /// <inheritdoc/>
    public override void Upcast(IEventMigrationBuilder<AudienceGranted, AudienceGrantedV1> builder) =>
        builder.Properties(properties => properties
            .DefaultValue(granted => granted.ClientId, SystemClientId.NotSet)
            .DefaultValue(granted => granted.Kind, SystemKind.None)
            .MapValues(granted => granted.Level, previous => previous.Level, map => map
                .Map(AudienceLevelV1.Tenant, AudienceLevel.Tenant)
                .Map(AudienceLevelV1.User, AudienceLevel.User)
                .Map(AudienceLevelV1.System, AudienceLevel.System)));

    /// <inheritdoc/>
    public override void Downcast(IEventMigrationBuilder<AudienceGrantedV1, AudienceGranted> builder) =>
        builder.Properties(properties => properties
            .MapValues(previous => previous.Level, granted => granted.Level, map => map
                .Map(AudienceLevel.Tenant, AudienceLevelV1.Tenant)
                .Map(AudienceLevel.User, AudienceLevelV1.User)
                .Map(AudienceLevel.System, AudienceLevelV1.System)
                .Map(AudienceLevel.NamedSystem, AudienceLevelV1.Tenant)));
}
