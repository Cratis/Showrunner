// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Web;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_S3ObjectStorage;

public class when_creating_a_download_link : Specification
{
    S3ObjectStorage _storage;
    DownloadLink _default;
    DownloadLink _short;
    DateTimeOffset _before;

    void Establish() => _storage = new(new()
    {
        Provider = ObjectStorageProviders.S3,
        Endpoint = "https://objects.example.com",
        Region = "se-sto1",
        Bucket = "backups",
        PathStyle = true,
        AccessKey = "access",
        SecretKey = "secret"
    });

    async Task Because()
    {
        _before = DateTimeOffset.UtcNow;
        _default = await _storage.CreateDownloadLinkAsync("ledger/Studio/acme/20260101T000000Z/Events.archive.gz", null, CancellationToken.None);
        _short = await _storage.CreateDownloadLinkAsync("ledger/Studio/acme/20260101T000000Z/Events.archive.gz", TimeSpan.FromSeconds(30), CancellationToken.None);
    }

    [Fact] void should_sign_the_link() => _default.Url.Query.ShouldContain("X-Amz-Signature=");
    [Fact] void should_address_the_bucket_in_the_path() => _default.Url.AbsolutePath.ShouldEqual("/backups/ledger/Studio/acme/20260101T000000Z/Events.archive.gz");
    [Fact] void should_use_the_endpoint() => _default.Url.Host.ShouldEqual("objects.example.com");
    [Fact] void should_last_five_minutes_by_default() => HttpUtility.ParseQueryString(_default.Url.Query)["X-Amz-Expires"].ShouldEqual("300");
    [Fact] void should_report_when_it_expires() => (_default.ExpiresAt - _before).TotalSeconds.ShouldBeInRange(299, 301);
    [Fact] void should_honor_a_shorter_time() => HttpUtility.ParseQueryString(_short.Url.Query)["X-Amz-Expires"].ShouldEqual("30");
    [Fact] void should_not_carry_the_secret_key() => _default.Url.ToString().Contains("secret", StringComparison.Ordinal).ShouldBeFalse();
}
#endif
