// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseVersion.when_comparing;

/// <summary>
/// Versions are ordered by their numbers, not by their text.
/// </summary>
public class versions_by_their_numbers : Specification
{
    [Fact] void should_order_a_newer_patch_after_an_older_one() => (new ReleaseVersion("1.2.4") > new ReleaseVersion("1.2.3")).ShouldBeTrue();

    [Fact] void should_order_a_newer_minor_after_an_older_one_even_when_the_text_sorts_the_other_way() => (new ReleaseVersion("1.10.0") > new ReleaseVersion("1.9.0")).ShouldBeTrue();

    [Fact] void should_order_a_newer_major_after_every_minor_of_the_older_one() => (new ReleaseVersion("2.0.0") > new ReleaseVersion("1.99.99")).ShouldBeTrue();

    [Fact] void should_treat_missing_numbers_as_zero() => ReleaseVersion.Compare("1.2", "1.2.0").ShouldEqual(0);

    [Fact] void should_ignore_a_leading_v() => ReleaseVersion.Compare("v1.2.0", "1.2.0").ShouldEqual(0);

    [Fact] void should_ignore_build_metadata() => ReleaseVersion.Compare("1.2.0+abc123", "1.2.0+def456").ShouldEqual(0);

    [Fact] void should_consider_a_version_not_newer_than_itself() => (new ReleaseVersion("1.2.0") <= new ReleaseVersion("1.2.0")).ShouldBeTrue();
}
#endif
