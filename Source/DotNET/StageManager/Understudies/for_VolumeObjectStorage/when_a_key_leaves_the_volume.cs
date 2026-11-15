// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_VolumeObjectStorage;

public class when_a_key_leaves_the_volume : Specification
{
    VolumeObjectStorage _storage;
    Exception? _write;
    Exception? _link;
    BucketState _state;

    void Establish() => _storage = new VolumeObjectStorage(Path.Combine(Path.GetTempPath(), $"understudy-volume-{Guid.NewGuid():N}"));

    async Task Because()
    {
        await using var content = new MemoryStream([1]);
        _write = await Catch.Exception(() => _storage.PutAsync("../escape", content, CancellationToken.None));
        _link = await Catch.Exception(() => _storage.CreateDownloadLinkAsync("a", null, CancellationToken.None));
        _state = await _storage.InspectAsync(CancellationToken.None);
    }

    [Fact] void should_refuse_the_key() => _write.ShouldBeOfExactType<ArgumentException>();
    [Fact] void should_have_no_signed_links() => _link.ShouldBeOfExactType<NotSupportedException>();
    [Fact] void should_prove_no_bucket_rule() => _state.ShouldEqual(BucketState.Unknown);
}
#endif
