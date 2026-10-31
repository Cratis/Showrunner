// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// An <see cref="IMeteringStore"/> in MongoDB, one collection beside the product's other data.
/// </summary>
/// <remarks>
/// A tenant's day is one document, so adding a quantity, remembering which event it came from and noting that a
/// correction is due is one atomic update. The same collection holds a cursor for each tenant and the lease.
/// </remarks>
/// <param name="database">The product's MongoDB database.</param>
/// <param name="options">The <see cref="MeteringOptions"/>.</param>
public class MongoMeteringStore(IMongoDatabase database, IOptions<MeteringOptions> options) : IMeteringStore
{
    const string LeaseId = "lease";
    const string DuplicateKey = "E11000";
    readonly IMongoCollection<BsonDocument> _collection = database.GetCollection<BsonDocument>(options.Value.Collection);

    /// <inheritdoc/>
    public async Task<bool> TryHold(string holder, TimeSpan lease, DateTimeOffset now)
    {
        var free = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("Holder", holder),
            Builders<BsonDocument>.Filter.Lt("Until", now.UtcDateTime));
        var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("_id", LeaseId), free);
        var update = Builders<BsonDocument>.Update.Set("Holder", holder).Set("Until", now.UtcDateTime + lease);

        try
        {
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return true;
        }
        catch (MongoWriteException exception) when (IsDuplicate(exception))
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<EventSequenceNumber> GetCursor(TenantId tenant)
    {
        var document = await _collection.Find(Builders<BsonDocument>.Filter.Eq("_id", CursorId(tenant))).FirstOrDefaultAsync();
        return document is null ? EventSequenceNumber.First : (ulong)document["Next"].ToInt64();
    }

    /// <inheritdoc/>
    public Task SetCursor(TenantId tenant, EventSequenceNumber next) =>
        _collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", CursorId(tenant)),
            Builders<BsonDocument>.Update.Set("Next", (long)next.Value),
            new UpdateOptions { IsUpsert = true });

    /// <inheritdoc/>
    public async Task<MeteringDay?> Get(TenantId tenant, ComputeDay day)
    {
        var document = await _collection.Find(Builders<BsonDocument>.Filter.Eq("_id", DayId(tenant, day))).FirstOrDefaultAsync();
        return document is null ? null : ToDay(document);
    }

    /// <inheritdoc/>
    public async Task<bool> Add(TenantId tenant, MeterId meter, MeterReading reading, EventSequenceNumber sequenceNumber, bool dueForCorrection)
    {
        var bucket = $"Buckets.{meter.Value}";
        var sequence = (long)sequenceNumber.Value;
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("_id", DayId(tenant, reading.Day)),
            Builders<BsonDocument>.Filter.Not(Builders<BsonDocument>.Filter.Gte($"{bucket}.LastApplied", sequence)));
        var update = Builders<BsonDocument>.Update
            .Inc($"{bucket}.Quantity", reading.Quantity.Value)
            .Set($"{bucket}.LastApplied", sequence)
            .SetOnInsert("Kind", "day")
            .SetOnInsert("Tenant", tenant.Value)
            .SetOnInsert("Day", DayText(reading.Day))
            .SetOnInsert("Closed", false)
            .Set("Organization", reading.Organization.Value.ToString("D", CultureInfo.InvariantCulture));
        if (dueForCorrection)
        {
            update = update.Max("CorrectionDue", sequence);
        }

        try
        {
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return true;
        }
        catch (MongoWriteException exception) when (IsDuplicate(exception))
        {
            // The day exists and the filter did not match it: this event was added to the bucket before.
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MeteringDay>> GetOpen(TenantId tenant, ComputeDay through)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("Kind", "day"),
            Builders<BsonDocument>.Filter.Eq("Tenant", tenant.Value),
            Builders<BsonDocument>.Filter.Eq("Closed", false),
            Builders<BsonDocument>.Filter.Lte("Day", DayText(through)));
        var documents = await _collection.Find(filter).Sort(Builders<BsonDocument>.Sort.Ascending("Day")).ToListAsync();
        return [.. documents.Select(ToDay)];
    }

    /// <inheritdoc/>
    public Task Close(TenantId tenant, ComputeDay day) =>
        _collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", DayId(tenant, day)),
            Builders<BsonDocument>.Update.Set("Closed", true));

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MeteringDay>> GetAwaitingCorrection(TenantId tenant)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("Kind", "day"),
            Builders<BsonDocument>.Filter.Eq("Tenant", tenant.Value),
            Builders<BsonDocument>.Filter.Eq("Closed", true),
            Builders<BsonDocument>.Filter.Ne("CorrectionDue", BsonNull.Value),
            Builders<BsonDocument>.Filter.Exists("CorrectionDue"));
        var documents = await _collection.Find(filter).Sort(Builders<BsonDocument>.Sort.Ascending("Day")).ToListAsync();
        return [.. documents.Select(ToDay)];
    }

    /// <inheritdoc/>
    public Task Corrected(TenantId tenant, ComputeDay day, EventSequenceNumber due) =>
        _collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", DayId(tenant, day)),
                Builders<BsonDocument>.Filter.Eq("CorrectionDue", (long)due.Value)),
            Builders<BsonDocument>.Update.Unset("CorrectionDue"));

    static bool IsDuplicate(MongoWriteException exception) =>
        exception.WriteError.Category == ServerErrorCategory.DuplicateKey
        || exception.WriteError.Message.Contains(DuplicateKey, StringComparison.Ordinal);

    static string CursorId(TenantId tenant) => $"cursor/{tenant.Value}";

    static string DayId(TenantId tenant, ComputeDay day) => $"day/{tenant.Value}/{DayText(day)}";

    static string DayText(ComputeDay day) => day.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    static MeteringDay ToDay(BsonDocument document)
    {
        var quantities = new Dictionary<MeterId, MeterQuantity>();
        foreach (var bucket in document["Buckets"].AsBsonDocument)
        {
            quantities[bucket.Name] = bucket.Value["Quantity"].ToInt64();
        }

        EventSequenceNumber? due = document.TryGetValue("CorrectionDue", out var value) && !value.IsBsonNull
            ? (ulong)value.ToInt64()
            : null;

        return new MeteringDay(
            document["Tenant"].AsString,
            DateOnly.ParseExact(document["Day"].AsString, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            new OrganizationId(Guid.Parse(document["Organization"].AsString, CultureInfo.InvariantCulture)),
            document["Closed"].AsBoolean,
            due,
            quantities);
    }
}
