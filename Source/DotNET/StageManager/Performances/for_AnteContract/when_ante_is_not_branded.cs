// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AnteContract;

public class when_ante_is_not_branded : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because() => _variables = AnteContract.EnvironmentVariables(PerformanceSamples.Performance());

    [Fact] void should_keep_the_logo_of_ante() => _variables.ContainsKey("Ante__LogoUrl").ShouldBeFalse();
    [Fact] void should_keep_the_stylesheet_of_ante() => _variables.ContainsKey("Ante__CustomCssUrl").ShouldBeFalse();
    [Fact] void should_keep_the_tab_title_of_ante() => _variables.ContainsKey("Ante__PageTitle").ShouldBeFalse();
    [Fact] void should_keep_the_splash_message_of_ante() => _variables.ContainsKey("Ante__SplashMessage").ShouldBeFalse();
    [Fact] void should_reserve_no_organization_names() => _variables.Keys.Any(key => key.Contains("ReservedNames", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_not_hold_sign_ups_for_review() => _variables.ContainsKey("Ante__Onboarding__RegistrationReview").ShouldBeFalse();
}
#endif
