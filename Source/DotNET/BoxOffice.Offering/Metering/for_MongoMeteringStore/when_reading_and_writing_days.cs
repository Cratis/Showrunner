// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_MongoMeteringStore;

public class when_reading_and_writing_days : Specification
{
    static readonly DateTimeOffset _now = new(2024, 5, 6, 7, 8, 9, TimeSpan.Zero);
    static readonly Guid _organization = Guid.Parse("11111111-2222-3333-4444-555555555555");

    readonly Dictionary<string, (BsonDocument Filter, BsonDocument? Update, BsonDocument? Sort)> _calls = new(StringComparer.Ordinal);
    readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);
    IMongoCollection<BsonDocument> _collection;
    List<BsonDocument> _found = [];
    MongoMeteringStore _store;

    static BsonDocument Render(FilterDefinition<BsonDocument> filter) =>
        filter.Render(new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry));

    static BsonDocument Render(UpdateDefinition<BsonDocument> update) =>
        update.Render(new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry)).AsBsonDocument;

    static BsonDocument Render(SortDefinition<BsonDocument> sort) =>
        sort.Render(new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry));

    static BsonDocument Expected(string json) => BsonDocument.Parse(json);

    static BsonDocument DayDocument(bool closed, string day, long? due, BsonDocument buckets)
    {
        var document = new BsonDocument
        {
            { "_id", $"day/t1/{day}" },
            { "Kind", "day" },
            { "Tenant", "t1" },
            { "Day", day },
            { "Closed", closed },
            { "Organization", _organization.ToString("D") },
            { "Buckets", buckets }
        };
        if (due is not null)
        {
            document["CorrectionDue"] = due.Value;
        }

        return document;
    }

    void Establish()
    {
        _collection = Substitute.For<IMongoCollection<BsonDocument>>();
        var database = Substitute.For<IMongoDatabase>();
        database.GetCollection<BsonDocument>("boxoffice_metering", Arg.Any<MongoCollectionSettings>()).Returns(_collection);
        _store = new MongoMeteringStore(database, Options.Create(new MeteringOptions()));
    }

    async Task Because()
    {
        string name = string.Empty;
        _collection.FindAsync(
            Arg.Any<FilterDefinition<BsonDocument>>(),
            Arg.Any<FindOptions<BsonDocument, BsonDocument>>(),
            Arg.Any<CancellationToken>()).Returns(call =>
            {
                var options = call.Arg<FindOptions<BsonDocument, BsonDocument>>();
                var cursor = Substitute.For<IAsyncCursor<BsonDocument>>();
                var batch = _found.ToList();
                cursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(batch.Count > 0), _ => Task.FromResult(false));
                cursor.Current.Returns(_ => batch);
                _calls[name] = (Render(call.Arg<FilterDefinition<BsonDocument>>()), null, options.Sort is null ? null : Render(options.Sort));
                return Task.FromResult(cursor);
            });
        _collection.UpdateOneAsync(
            Arg.Any<FilterDefinition<BsonDocument>>(),
            Arg.Any<UpdateDefinition<BsonDocument>>(),
            Arg.Any<UpdateOptions>(),
            Arg.Any<CancellationToken>()).Returns(call =>
            {
                var options = call.Arg<UpdateOptions>();
                _calls[name] = (Render(call.Arg<FilterDefinition<BsonDocument>>()), Render(call.Arg<UpdateDefinition<BsonDocument>>()), options?.IsUpsert == true ? new BsonDocument("upsert", true) : null);
                return Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 1, null));
            });

        var day = new DateOnly(2024, 5, 4);
        var reading = new MeterReading(new OrganizationId(_organization), day, 7);

        name = "hold";
        _results[name] = await _store.TryHold("me", TimeSpan.FromMinutes(2), _now);
        name = "set_cursor";
        await _store.SetCursor("t1", 12UL);
        name = "add";
        _results[name] = await _store.Add("t1", "build-minutes", reading, 5UL, false);
        name = "add_due";
        await _store.Add("t1", "build-minutes", reading, 5UL, true);
        name = "close";
        await _store.Close("t1", day);
        name = "corrected";
        await _store.Corrected("t1", day, 5UL);

        _found = [DayDocument(false, "2024-05-04", 8, new BsonDocument("build-minutes", new BsonDocument { { "Quantity", 42L }, { "LastApplied", 9L } }))];
        name = "open";
        _results[name] = await _store.GetOpen("t1", new DateOnly(2024, 5, 6));
        name = "awaiting";
        _results[name] = await _store.GetAwaitingCorrection("t1");
        name = "get";
        _results[name] = await _store.Get("t1", day);
        _found = [new BsonDocument("Next", 13L)];
        name = "cursor";
        _results[name] = await _store.GetCursor("t1");

        _found = [];
        name = "none_get";
        _results[name] = await _store.Get("t1", day);
        name = "none_cursor";
        _results[name] = await _store.GetCursor("t1");
    }

    [Fact] void should_hold_the_lease_when_it_is_free() => ((bool)_results["hold"]!).ShouldBeTrue();

    [Fact]
    void should_take_the_lease_by_upsert_when_the_holder_is_me_or_it_expired() => _calls["hold"].ShouldEqual((
        Expected("{\"_id\": \"lease\", \"$or\": [{\"Holder\": \"me\"}, {\"Until\": {\"$lt\": {\"$date\": \"2024-05-06T07:08:09Z\"}}}]}"),
        Expected("{\"$set\": {\"Holder\": \"me\", \"Until\": {\"$date\": \"2024-05-06T07:10:09Z\"}}}"),
        new BsonDocument("upsert", true)));

    [Fact]
    void should_set_the_cursor_by_upsert() => _calls["set_cursor"].ShouldEqual((
        Expected("{\"_id\": \"cursor/t1\"}"),
        Expected("{\"$set\": {\"Next\": {\"$numberLong\": \"12\"}}}"),
        new BsonDocument("upsert", true)));

    [Fact] void should_add_the_quantity() => ((bool)_results["add"]!).ShouldBeTrue();

    [Fact]
    void should_add_to_the_bucket_of_the_day_unless_the_event_was_applied() => _calls["add"].ShouldEqual((
        Expected("{\"_id\": \"day/t1/2024-05-04\", \"Buckets.build-minutes.LastApplied\": {\"$not\": {\"$gte\": {\"$numberLong\": \"5\"}}}}"),
        Expected("{\"$inc\": {\"Buckets.build-minutes.Quantity\": {\"$numberLong\": \"7\"}}, \"$set\": {\"Buckets.build-minutes.LastApplied\": {\"$numberLong\": \"5\"}, \"Organization\": \"11111111-2222-3333-4444-555555555555\"}, \"$setOnInsert\": {\"Kind\": \"day\", \"Tenant\": \"t1\", \"Day\": \"2024-05-04\", \"Closed\": false}}"),
        new BsonDocument("upsert", true)));

    [Fact]
    void should_note_the_correction_that_is_due_when_asked_to() => _calls["add_due"].Update!.ShouldEqual(
        Expected("{\"$inc\": {\"Buckets.build-minutes.Quantity\": {\"$numberLong\": \"7\"}}, \"$set\": {\"Buckets.build-minutes.LastApplied\": {\"$numberLong\": \"5\"}, \"Organization\": \"11111111-2222-3333-4444-555555555555\"}, \"$setOnInsert\": {\"Kind\": \"day\", \"Tenant\": \"t1\", \"Day\": \"2024-05-04\", \"Closed\": false}, \"$max\": {\"CorrectionDue\": {\"$numberLong\": \"5\"}}}"));

    [Fact]
    void should_close_the_day() => _calls["close"].ShouldEqual((
        Expected("{\"_id\": \"day/t1/2024-05-04\"}"),
        Expected("{\"$set\": {\"Closed\": true}}"),
        null));

    [Fact]
    void should_clear_the_correction_only_when_it_is_the_one_that_was_due() => _calls["corrected"].ShouldEqual((
        Expected("{\"_id\": \"day/t1/2024-05-04\", \"CorrectionDue\": {\"$numberLong\": \"5\"}}"),
        Expected("{\"$unset\": {\"CorrectionDue\": 1}}"),
        null));

    [Fact]
    void should_find_open_days_of_the_tenant_up_to_the_day_oldest_first() => (_calls["open"].Filter, _calls["open"].Sort).ShouldEqual((
        Expected("{\"Kind\": \"day\", \"Tenant\": \"t1\", \"Closed\": false, \"Day\": {\"$lte\": \"2024-05-06\"}}"),
        (BsonDocument?)Expected("{\"Day\": 1}")));

    [Fact]
    void should_find_closed_days_with_a_correction_due_oldest_first() => (_calls["awaiting"].Filter, _calls["awaiting"].Sort).ShouldEqual((
        Expected("{\"Kind\": \"day\", \"Tenant\": \"t1\", \"Closed\": true, \"CorrectionDue\": {\"$ne\": null, \"$exists\": true}}"),
        (BsonDocument?)Expected("{\"Day\": 1}")));

    [Fact] void should_read_the_open_days() => DaysEqual(ToDay(false, 8UL), ((IReadOnlyList<MeteringDay>)_results["open"]!).Single()).ShouldBeTrue();
    [Fact] void should_read_the_days_awaiting_correction() => DaysEqual(ToDay(false, 8UL), ((IReadOnlyList<MeteringDay>)_results["awaiting"]!).Single()).ShouldBeTrue();
    [Fact] void should_read_a_day_by_its_id() => _calls["get"].Filter.ShouldEqual(Expected("{\"_id\": \"day/t1/2024-05-04\"}"));
    [Fact] void should_map_the_day() => DaysEqual(ToDay(false, 8UL), (MeteringDay)_results["get"]!).ShouldBeTrue();
    [Fact] void should_read_the_cursor_by_its_id() => _calls["cursor"].Filter.ShouldEqual(Expected("{\"_id\": \"cursor/t1\"}"));
    [Fact] void should_read_the_next_event_of_the_cursor() => ((EventSequenceNumber)_results["cursor"]!).ShouldEqual((EventSequenceNumber)13UL);
    [Fact] void should_find_no_day() => _results["none_get"].ShouldBeNull();
    [Fact] void should_start_a_cursor_at_the_first_event() => ((EventSequenceNumber)_results["none_cursor"]!).ShouldEqual(EventSequenceNumber.First);

    static MeteringDay ToDay(bool closed, ulong due) => new(
        "t1",
        new DateOnly(2024, 5, 4),
        new OrganizationId(_organization),
        closed,
        due,
        new Dictionary<MeterId, MeterQuantity> { ["build-minutes"] = 42 });

    static bool DaysEqual(MeteringDay expected, MeteringDay actual) =>
        expected.Tenant == actual.Tenant
        && expected.Day == actual.Day
        && expected.Organization == actual.Organization
        && expected.IsClosed == actual.IsClosed
        && expected.CorrectionDue == actual.CorrectionDue
        && expected.Quantities.OrderBy(_ => _.Key.Value).SequenceEqual(actual.Quantities.OrderBy(_ => _.Key.Value));
}
#endif
