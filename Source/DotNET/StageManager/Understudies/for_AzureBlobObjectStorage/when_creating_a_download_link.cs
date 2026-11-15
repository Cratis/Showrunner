// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Web;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_AzureBlobObjectStorage;

public class when_creating_a_download_link : Specification
{
    AzureBlobObjectStorage _storage;
    DownloadLink _link;
    BucketState _state;
    bool _expiry;
    DateTimeOffset _before;

    void Establish() => _storage = new(new()
    {
        Provider = ObjectStorageProviders.AzureBlob,
        AccountName = "devstoreaccount1",
        AccountKey = Convert.ToBase64String(new byte[32]),
        Bucket = "backups",
        BlobServiceUrl = "http://127.0.0.1:10000/devstoreaccount1"
    });

    async Task Because()
    {
        _before = DateTimeOffset.UtcNow;
        _link = await _storage.CreateDownloadLinkAsync("ledger/Studio/acme/20260101T000000Z/Events.archive.gz", null, CancellationToken.None);
        _state = await _storage.InspectAsync(CancellationToken.None);
        _expiry = await _storage.TryEnforceExpiryAsync(35, CancellationToken.None);
    }

    [Fact] void should_sign_the_link() => _link.Url.Query.ShouldContain("sig=");
    [Fact] void should_only_allow_reading() => HttpUtility.ParseQueryString(_link.Url.Query)["sp"].ShouldEqual("r");
    [Fact] void should_point_at_the_blob() => _link.Url.AbsolutePath.ShouldEqual("/devstoreaccount1/backups/ledger/Studio/acme/20260101T000000Z/Events.archive.gz");
    [Fact] void should_last_five_minutes_by_default() => (_link.ExpiresAt - _before).TotalSeconds.ShouldBeInRange(299, 301);
    [Fact] void should_not_prove_any_bucket_rule() => _state.ShouldEqual(BucketState.Unknown);
    [Fact] void should_not_apply_an_expiry_rule() => _expiry.ShouldBeFalse();
}
#endif
