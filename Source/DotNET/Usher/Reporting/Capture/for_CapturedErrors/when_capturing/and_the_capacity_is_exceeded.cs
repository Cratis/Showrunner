// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_the_capacity_is_exceeded : a_store
{
    CorrelationId _oldest;
    CorrelationId _middle;
    bool _foundOldest;
    bool _foundMiddle;
    bool _foundNewest;

    void Establish()
    {
        _options.ErrorCapture.Capacity = 2;
        _oldest = Guid.NewGuid();
        _middle = Guid.NewGuid();
    }

    void Because()
    {
        _store.Capture(Owner, _oldest, Error);
        _store.Capture(Owner, _middle, Error);
        _store.Capture(Owner, _correlationId, Error);
        _foundOldest = _store.TryGet(Owner, _oldest, out _);
        _foundMiddle = _store.TryGet(Owner, _middle, out _);
        _foundNewest = _store.TryGet(Owner, _correlationId, out _);
    }

    [Fact] void should_forget_the_oldest() => _foundOldest.ShouldBeFalse();
    [Fact] void should_keep_the_one_after_the_oldest() => _foundMiddle.ShouldBeTrue();
    [Fact] void should_keep_the_newest() => _foundNewest.ShouldBeTrue();
}
#endif
