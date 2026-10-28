// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.when_a_command_fails;

public class and_usher_is_not_added : a_failing_command
{
    ServiceProvider _provider;
    Exception? _error;

    void Because()
    {
        _provider = new ServiceCollection().BuildServiceProvider();
        _error = Catch.Exception(() => Fail(_provider));
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_not_fail() => _error.ShouldBeNull();
    [Fact] void should_leave_the_failure_to_arc() => _mapped.ShouldBeFalse();
}
#endif
