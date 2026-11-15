// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_validating_schedules : Specification
{
    static IReadOnlyList<UnderstudyProblem> Validate(string schedule)
    {
        var spec = UnderstudySamples.Spec();
        spec.Schedule = schedule;
        return UnderstudyValidator.Validate(spec);
    }

    [Fact] void should_accept_every_hour() => Validate("0 * * * *").ShouldBeEmpty();
    [Fact] void should_accept_steps_and_lists() => Validate("*/15 1,13 * * 1-5").ShouldBeEmpty();
    [Fact] void should_accept_a_macro() => Validate("@daily").ShouldBeEmpty();
    [Fact] void should_refuse_too_few_fields() => Validate("0 * * *").ShouldNotBeEmpty();
    [Fact] void should_refuse_too_many_fields() => Validate("0 0 * * * *").ShouldNotBeEmpty();
    [Fact] void should_refuse_text() => Validate("hourly please").ShouldNotBeEmpty();
    [Fact] void should_refuse_empty() => Validate(string.Empty).ShouldNotBeEmpty();
}
#endif
