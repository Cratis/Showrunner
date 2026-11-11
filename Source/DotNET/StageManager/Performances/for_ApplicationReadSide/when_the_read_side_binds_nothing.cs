// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationReadSide;

public class when_the_read_side_binds_nothing : Specification
{
    ApplicationReadSide _readSide;

    void Because() => _readSide = ApplicationReadSide.None;

    [Fact] void should_have_no_mount() => _readSide.Mount.ShouldBeNull();
    [Fact] void should_need_no_secret() => _readSide.Secrets.Count().ShouldEqual(0);
    [Fact] void should_have_no_environment_variable() => _readSide.EnvironmentVariables.Count.ShouldEqual(0);
    [Fact] void should_have_no_secret_environment_variable() => _readSide.SecretEnvironmentVariables.Count.ShouldEqual(0);
    [Fact] void should_not_want_the_graph() => _readSide.GraphWanted.ShouldBeFalse();
    [Fact] void should_have_nothing_unreadable() => _readSide.Unreadable.Count.ShouldEqual(0);
}
#endif
