// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_the_retention_has_passed : a_store
{
    bool _foundJustBefore;
    bool _foundAfter;

    void Because()
    {
        _store.Capture(Owner, _correlationId, Error);
        _clock.Advance(_options.ErrorCapture.Retention - TimeSpan.FromSeconds(1));
        _foundJustBefore = _store.TryGet(Owner, _correlationId, out _);
        _clock.Advance(TimeSpan.FromSeconds(1));
        _foundAfter = _store.TryGet(Owner, _correlationId, out _);
    }

    [Fact] void should_find_it_until_the_retention_has_passed() => _foundJustBefore.ShouldBeTrue();
    [Fact] void should_not_find_it_after() => _foundAfter.ShouldBeFalse();
}
#endif
