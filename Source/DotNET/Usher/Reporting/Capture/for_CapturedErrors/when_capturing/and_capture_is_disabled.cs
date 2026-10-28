// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_capture_is_disabled : a_store
{
    bool _found;

    void Establish() => _options.ErrorCapture.Enabled = false;

    void Because()
    {
        _store.Capture(Owner, _correlationId, Error);
        _found = _store.TryGet(Owner, _correlationId, out _);
    }

    [Fact] void should_not_keep_it() => _found.ShouldBeFalse();
}
#endif
