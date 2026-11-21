// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Configuration.for_DurationText;

public class when_parsing_durations : Specification
{
    static readonly string[] BadInputs = ["", "0", "-5m", "5x", "m"];

    TimeSpan _seconds;
    TimeSpan _minutes;
    TimeSpan _hours;
    TimeSpan _days;
    TimeSpan _bare;
    bool _bareAccepted;
    bool _nullAccepted;
    List<string> _accepted;

    void Because()
    {
        _ = DurationText.TryParse("90s", out _seconds);
        _ = DurationText.TryParse("30m", out _minutes);
        _ = DurationText.TryParse("3h", out _hours);
        _ = DurationText.TryParse("2d", out _days);
        _bareAccepted = DurationText.TryParse("21600", out _bare);
        _nullAccepted = DurationText.TryParse(null, out _);
        _accepted = [.. BadInputs.Where(input => DurationText.TryParse(input, out _))];
    }

    [Fact] void should_read_seconds() => _seconds.ShouldEqual(TimeSpan.FromSeconds(90));
    [Fact] void should_read_minutes() => _minutes.ShouldEqual(TimeSpan.FromMinutes(30));
    [Fact] void should_read_hours() => _hours.ShouldEqual(TimeSpan.FromHours(3));
    [Fact] void should_read_days() => _days.ShouldEqual(TimeSpan.FromDays(2));
    [Fact] void should_accept_a_bare_number() => _bareAccepted.ShouldBeTrue();
    [Fact] void should_read_a_bare_number_as_seconds_like_the_python_environment() => _bare.ShouldEqual(TimeSpan.FromHours(6));
    [Fact] void should_not_accept_nothing() => _nullAccepted.ShouldBeFalse();
    [Fact] void should_not_accept_zero_negative_or_unknown_units() => _accepted.ShouldBeEmpty();
}
#endif
