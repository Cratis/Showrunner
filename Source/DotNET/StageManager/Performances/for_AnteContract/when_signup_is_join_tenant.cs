// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AnteContract;

public class when_signup_is_join_tenant : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.Onboarding.SignUp = SignUpMode.JoinTenant;
        _variables = AnteContract.EnvironmentVariables(performance);
    }

    [Fact] void should_close_registration() => _variables["Ante__Onboarding__RegistrationEnabled"].ShouldEqual("false");
    [Fact] void should_say_join_tenant() => _variables["Ante__Onboarding__SignUp"].ShouldEqual("JoinTenant");
}
#endif
