// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantToLapse;

public class when_checking_if_a_grant_is_due : Specification
{
    static readonly DateTimeOffset _end = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    readonly GrantToLapse _grant = new(IssueId.New(), _end);

    [Fact] void should_not_be_due_before_its_end() => _grant.IsDue(_end.AddTicks(-1)).ShouldBeFalse();
    [Fact] void should_be_due_at_its_end() => _grant.IsDue(_end).ShouldBeTrue();
    [Fact] void should_be_due_after_its_end() => _grant.IsDue(_end.AddMinutes(1)).ShouldBeTrue();
}
#endif
