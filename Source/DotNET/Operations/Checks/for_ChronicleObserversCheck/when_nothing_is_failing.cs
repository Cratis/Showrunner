// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_nothing_is_failing : a_chronicle_observers_check
{
    async Task Because() => await Run();

    [Fact] void should_find_nothing() => _findings.ShouldBeEmpty();
    [Fact] void should_give_every_request_a_timeout_within_the_request_limit() => _chronicle.Timeouts.All(timeout => timeout > TimeSpan.Zero && timeout <= TimeSpan.FromSeconds(10)).ShouldBeTrue();
}
#endif
