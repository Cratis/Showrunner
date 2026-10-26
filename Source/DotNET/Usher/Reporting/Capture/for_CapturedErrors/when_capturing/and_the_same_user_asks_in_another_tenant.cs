// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.when_capturing;

public class and_the_same_user_asks_in_another_tenant : a_store
{
    bool _found;

    void Because()
    {
        _store.Capture(Owner, _correlationId, Error);
        _found = _store.TryGet(Owner with { Tenant = "another-tenant" }, _correlationId, out _);
    }

    [Fact] void should_not_find_it() => _found.ShouldBeFalse();
}
#endif
