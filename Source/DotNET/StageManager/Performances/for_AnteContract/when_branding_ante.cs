// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AnteContract;

public class when_branding_ante : Specification
{
    IReadOnlyDictionary<string, string> _variables;

    void Because() => _variables = AnteContract.EnvironmentVariables(ProxySamples.ConfiguredApplication());

    [Fact] void should_set_the_logo() => _variables["Ante__LogoUrl"].ShouldEqual("/brand/logo.svg");
    [Fact] void should_set_the_stylesheet() => _variables["Ante__CustomCssUrl"].ShouldEqual("/brand/lobby.css");
    [Fact] void should_set_the_tab_title() => _variables["Ante__PageTitle"].ShouldEqual("Crm");
    [Fact] void should_set_the_splash_message() => _variables["Ante__SplashMessage"].ShouldEqual("Loading Crm");
    [Fact] void should_reserve_the_first_name() => _variables["Ante__Organization__ReservedNames__0"].ShouldEqual("admin");
    [Fact] void should_reserve_the_second_name() => _variables["Ante__Organization__ReservedNames__1"].ShouldEqual("api");
    [Fact] void should_hold_sign_ups_for_review() => _variables["Ante__Onboarding__RegistrationReview"].ShouldEqual("true");
}
#endif
