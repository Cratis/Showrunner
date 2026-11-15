// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_VolumeObjectStorage;

public class when_writing_listing_reading_and_deleting : Specification
{
    string _root;
    VolumeObjectStorage _storage;
    List<StoredObject> _underProduction;
    List<StoredObject> _everything;
    string _read;
    Stream? _missing;
    List<StoredObject> _afterDelete;

    void Establish()
    {
        _root = Path.Combine(Path.GetTempPath(), $"understudy-volume-{Guid.NewGuid():N}");
        _storage = new VolumeObjectStorage(_root);
    }

    async Task Because()
    {
        await Write("ledger/Studio/acme/20260101T000000Z/Events.archive.gz", "events");
        await Write("ledger/Studio/acme/20260101T000000Z/manifest.json", "{}");
        await Write("other/Studio/acme/20260101T000000Z/manifest.json", "{}");

        _underProduction = await List("ledger/");
        _everything = await List(string.Empty);
        await using (var stream = (await _storage.OpenReadAsync("ledger/Studio/acme/20260101T000000Z/Events.archive.gz", CancellationToken.None))!)
        {
            using var reader = new StreamReader(stream);
            _read = await reader.ReadToEndAsync();
        }

        _missing = await _storage.OpenReadAsync("ledger/nothing", CancellationToken.None);
        await _storage.DeleteAsync("ledger/Studio/acme/20260101T000000Z/manifest.json", CancellationToken.None);
        await _storage.DeleteAsync("ledger/never-existed", CancellationToken.None);
        _afterDelete = await List("ledger/");
    }

    void Destroy() => Directory.Delete(_root, true);

    [Fact] void should_list_only_what_is_under_the_prefix() => _underProduction.Select(item => item.Key).ShouldEqual(
        ["ledger/Studio/acme/20260101T000000Z/Events.archive.gz", "ledger/Studio/acme/20260101T000000Z/manifest.json"]);
    [Fact] void should_list_everything_without_a_prefix() => _everything.Count.ShouldEqual(3);
    [Fact] void should_report_the_size() => _underProduction[0].Size.ShouldEqual(Encoding.UTF8.GetByteCount("events"));
    [Fact] void should_read_back_what_was_written() => _read.ShouldEqual("events");
    [Fact] void should_have_nothing_to_read_for_a_missing_key() => _missing.ShouldBeNull();
    [Fact] void should_delete_an_object() => _afterDelete.Count.ShouldEqual(1);
    [Fact] void should_not_have_signed_links() => _storage.SupportsSignedLinks.ShouldBeFalse();

    async Task Write(string key, string content)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _storage.PutAsync(key, stream, CancellationToken.None);
    }

    async Task<List<StoredObject>> List(string prefix)
    {
        var items = new List<StoredObject>();
        await foreach (var item in _storage.ListAsync(prefix, CancellationToken.None))
        {
            items.Add(item);
        }

        return items;
    }
}
#endif
