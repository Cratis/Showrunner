// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_the_correlation_id_is_captured_twice : a_store
{
    CapturedError? _error;

    void Because()
    {
        _store.Capture(Owner, _correlationId, Error);
        _store.Capture(Owner, _correlationId, Error with { ErrorMessage = "A later failure in the same operation" });
        _store.TryGet(Owner, _correlationId, out _error);
    }

    [Fact] void should_keep_the_first() => _error.ShouldEqual(Error);
}
#endif
