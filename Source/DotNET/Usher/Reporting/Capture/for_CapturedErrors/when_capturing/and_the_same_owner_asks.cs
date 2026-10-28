// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_the_same_owner_asks : a_store
{
    bool _found;
    CapturedError? _error;

    void Because()
    {
        _store.Capture(Owner, _correlationId, Error);
        _found = _store.TryGet(Owner, _correlationId, out _error);
    }

    [Fact] void should_find_it() => _found.ShouldBeTrue();
    [Fact] void should_give_the_captured_error() => _error.ShouldEqual(Error);
}
#endif
