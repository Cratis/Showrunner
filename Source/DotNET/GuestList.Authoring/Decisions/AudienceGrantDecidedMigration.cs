// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Events.Migrations;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Migrates <see cref="AudienceGrantDecided"/> between generation 1 and generation 2. A decision stored before systems were
/// named names none, so it stays the product-wide <see cref="AudienceLevel.System"/> grant.
/// </summary>
public class AudienceGrantDecidedMigration : EventTypeMigration<AudienceGrantDecided, AudienceGrantDecidedV1>
{
    /// <inheritdoc/>
    public override void Upcast(IEventMigrationBuilder<AudienceGrantDecided, AudienceGrantDecidedV1> builder) =>
        builder.Properties(properties => properties
            .DefaultValue(decided => decided.ClientId, SystemClientId.NotSet)
            .DefaultValue(decided => decided.Kind, SystemKind.None)
            .MapValues(decided => decided.Level, previous => previous.Level, map => map
                .Map(AudienceLevelV1.Tenant, AudienceLevel.Tenant)
                .Map(AudienceLevelV1.User, AudienceLevel.User)
                .Map(AudienceLevelV1.System, AudienceLevel.System)));

    /// <inheritdoc/>
    public override void Downcast(IEventMigrationBuilder<AudienceGrantDecidedV1, AudienceGrantDecided> builder) =>
        builder.Properties(properties => properties
            .MapValues(previous => previous.Level, decided => decided.Level, map => map
                .Map(AudienceLevel.Tenant, AudienceLevelV1.Tenant)
                .Map(AudienceLevel.User, AudienceLevelV1.User)
                .Map(AudienceLevel.System, AudienceLevelV1.System)
                .Map(AudienceLevel.NamedSystem, AudienceLevelV1.Tenant)));
}
