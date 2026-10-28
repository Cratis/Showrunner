// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.given;
using Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.when_a_command_fails;

public class and_someone_is_signed_in : a_failing_command
{
    ServiceProvider _provider;
    CapturedError? _captured;

    void Because()
    {
        _provider = _services.BuildServiceProvider();
        Fail(_provider);
        _captured = CapturedIn(_provider);
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_leave_the_failure_to_arc() => _mapped.ShouldBeFalse();
    [Fact] void should_not_give_a_result() => _result.ShouldBeNull();
    [Fact] void should_capture_where_the_command_belongs() => _captured!.Location.ShouldEqual(new SliceLocation("Invoicing", "Invoices", "SendInvoice"));
    [Fact] void should_capture_the_error_type() => _captured!.ErrorType.Value.ShouldEqual("System.InvalidOperationException");
    [Fact] void should_capture_the_message() => _captured!.ErrorMessage.Value.ShouldEqual(Thrower.Message);
    [Fact] void should_capture_the_stack_frames() => _captured!.StackFrames.First().Method.Value.ShouldEqual($"{typeof(Thrower).FullName}.Throw");
    [Fact] void should_capture_the_configured_version() => _captured!.ApplicationVersion.Value.ShouldEqual("4.12.1");
}
#endif
