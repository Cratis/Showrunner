// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.when_a_command_fails;

public class and_no_one_is_signed_in : a_failing_command
{
    ServiceProvider _provider;
    CapturedError? _captured;

    void Establish() => _owners.TryGet(out Arg.Any<ErrorOwner?>()).Returns(false);

    void Because()
    {
        _provider = _services.BuildServiceProvider();
        Fail(_provider);
        _captured = CapturedIn(_provider);
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_leave_the_failure_to_arc() => _mapped.ShouldBeFalse();
    [Fact] void should_not_capture_it() => _captured.ShouldBeNull();
}
#endif
