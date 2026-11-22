// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseVersion.when_comparing;

/// <summary>
/// A value that is not a version sorts before every version, and is not valid.
/// </summary>
public class values_that_are_not_versions : Specification
{
    [Fact] void should_sort_before_a_version() => (new ReleaseVersion("0.0.1") > new ReleaseVersion("latest")).ShouldBeTrue();

    [Fact] void should_sort_none_before_a_version() => (new ReleaseVersion("0.0.1") > ReleaseVersion.None).ShouldBeTrue();

    [Fact] void should_not_be_valid_when_text() => new ReleaseVersion("latest").IsValid.ShouldBeFalse();

    [Fact] void should_not_be_valid_when_it_has_four_numbers() => new ReleaseVersion("1.2.3.4").IsValid.ShouldBeFalse();

    [Fact] void should_not_be_valid_when_empty() => ReleaseVersion.None.IsValid.ShouldBeFalse();

    [Fact] void should_not_be_set_when_empty() => ReleaseVersion.None.IsSet.ShouldBeFalse();

    [Fact] void should_be_valid_when_a_prerelease() => new ReleaseVersion("1.3.0-beta.1").IsValid.ShouldBeTrue();
}
#endif
