// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseVersion.when_comparing;

/// <summary>
/// A prerelease comes before the release it leads up to, and prereleases are ordered by their identifiers.
/// </summary>
public class prereleases : Specification
{
    [Fact] void should_come_before_their_release() => (new ReleaseVersion("2.0.0-rc.1") < new ReleaseVersion("2.0.0")).ShouldBeTrue();

    [Fact] void should_come_after_the_previous_release() => (new ReleaseVersion("2.0.0-alpha") > new ReleaseVersion("1.9.9")).ShouldBeTrue();

    [Fact] void should_order_numeric_identifiers_by_number() => (new ReleaseVersion("2.0.0-beta.11") > new ReleaseVersion("2.0.0-beta.2")).ShouldBeTrue();

    [Fact] void should_order_a_numeric_identifier_before_a_text_one() => (new ReleaseVersion("2.0.0-1") < new ReleaseVersion("2.0.0-alpha")).ShouldBeTrue();

    [Fact] void should_order_text_identifiers_alphabetically() => (new ReleaseVersion("2.0.0-alpha") < new ReleaseVersion("2.0.0-beta")).ShouldBeTrue();

    [Fact] void should_order_a_longer_prerelease_after_its_prefix() => (new ReleaseVersion("2.0.0-beta") < new ReleaseVersion("2.0.0-beta.1")).ShouldBeTrue();
}
#endif
