// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Integration.given;

/// <summary>
/// A metering store over a database of its own.
/// </summary>
[Collection(MongoCollection.Name)]
public class a_metering_store : Specification
{
    protected static readonly DateOnly _day = new(2026, 10, 30);
    protected MongoMeteringStore _store;
    protected TenantId _tenant;
    protected OrganizationId _organization;

    void Establish()
    {
        _store = NewStore();
        _tenant = "acme";
        _organization = OrganizationId.New();
    }

    /// <summary>
    /// Creates a store over the same database, as a restart of the product would.
    /// </summary>
    /// <returns>The store.</returns>
    protected MongoMeteringStore NewStore() => new(MongoInfrastructure.Current.NewDatabase(), Options.Create(new MeteringOptions()));

    /// <summary>
    /// Creates a reading of the day.
    /// </summary>
    /// <param name="minutes">The quantity.</param>
    /// <returns>The reading.</returns>
    protected MeterReading Reading(long minutes) => new(_organization, _day, minutes);
}
