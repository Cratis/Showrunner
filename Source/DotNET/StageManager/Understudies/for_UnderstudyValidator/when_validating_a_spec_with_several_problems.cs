// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_validating_a_spec_with_several_problems : Specification
{
    IReadOnlyList<UnderstudyProblem> _problems;

    void Because()
    {
        var spec = new UnderstudySpec { Schedule = "every day", MaxAgeDays = 0, Storage = new() { Provider = "S3" } };
        spec.Retention.Hourly = -1;
        _problems = UnderstudyValidator.Validate(spec);
    }

    [Fact] void should_say_which_production_is_missing() => _problems.Select(problem => problem.Reason).ShouldContain("ProductionRefMissing");
    [Fact] void should_refuse_a_schedule_that_is_not_cron() => _problems.Select(problem => problem.Reason).ShouldContain("InvalidSchedule");
    [Fact] void should_refuse_no_maximum_age() => _problems.Select(problem => problem.Reason).ShouldContain("InvalidMaxAge");
    [Fact] void should_refuse_a_negative_tier() => _problems.Select(problem => problem.Reason).ShouldContain("InvalidRetention");
    [Fact] void should_ask_for_a_bucket() => _problems.Select(problem => problem.Reason).ShouldContain("BucketMissing");
    [Fact] void should_ask_for_credentials() => _problems.Select(problem => problem.Reason).ShouldContain("CredentialsMissing");
}
#endif
