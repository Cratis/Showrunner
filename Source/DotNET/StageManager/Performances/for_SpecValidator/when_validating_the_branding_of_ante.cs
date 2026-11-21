// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_the_branding_of_ante : Specification
{
    static IReadOnlyList<string> Validate(Action<OnboardingSpec> change)
    {
        var performance = ProxySamples.ConfiguredApplication();
        change(performance.Spec.Onboarding);
        return SpecValidator.Validate(performance);
    }

    [Fact] void should_accept_an_https_logo() => Validate(onboarding => onboarding.Branding!.LogoUrl = "https://cdn.example.com/logo.svg").Count.ShouldEqual(0);
    [Fact] void should_refuse_a_logo_over_http() => Validate(onboarding => onboarding.Branding!.LogoUrl = "http://cdn.example.com/logo.svg").Count.ShouldEqual(1);
    [Fact] void should_refuse_a_stylesheet_on_another_scheme() => Validate(onboarding => onboarding.Branding!.CustomCssUrl = "ftp://cdn.example.com/lobby.css").Count.ShouldEqual(1);
    [Fact] void should_refuse_assets_without_files() => Validate(onboarding => onboarding.Branding!.Assets!.Files = []).Count.ShouldEqual(1);
    [Fact] void should_refuse_assets_without_a_config_map() => Validate(onboarding => onboarding.Branding!.Assets!.ConfigMapRef = new()).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_file_that_is_a_path() => Validate(onboarding => onboarding.Branding!.Assets!.Files = ["../logo.svg"]).Count.ShouldEqual(1);
    [Fact] void should_refuse_a_file_named_twice() => Validate(onboarding => onboarding.Branding!.Assets!.Files = ["logo.svg", "logo.svg"]).Count.ShouldEqual(1);
    [Fact] void should_refuse_an_empty_reserved_name() => Validate(onboarding => onboarding.ReservedOrganizationNames = ["admin", " "]).Count.ShouldEqual(1);
}
#endif
