// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_EmailRetryOptions.when_asking_for_the_delay;

public class after_failures : Specification
{
    readonly EmailRetryOptions _retry = new() { InitialDelay = TimeSpan.FromSeconds(30), MaxDelay = TimeSpan.FromMinutes(5) };

    [Fact] void should_wait_the_initial_delay_after_the_first() => Assert.Equal(TimeSpan.FromSeconds(30), _retry.DelayAfter(1));
    [Fact] void should_double_after_each() => Assert.Equal(TimeSpan.FromMinutes(2), _retry.DelayAfter(3));
    [Fact] void should_never_wait_longer_than_the_longest() => Assert.Equal(TimeSpan.FromMinutes(5), _retry.DelayAfter(20));
    [Fact] void should_cope_with_a_great_many_failures() => Assert.Equal(TimeSpan.FromMinutes(5), _retry.DelayAfter(int.MaxValue));
}
#endif
