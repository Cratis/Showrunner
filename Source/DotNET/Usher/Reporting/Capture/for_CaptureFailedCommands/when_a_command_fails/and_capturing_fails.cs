// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.given;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.when_a_command_fails;

public class and_capturing_fails : a_failing_command
{
    ServiceProvider _provider;
    Exception? _error;

    void Establish()
    {
        var errors = Substitute.For<ICapturedErrors>();
        errors.When(_ => _.Capture(Arg.Any<ErrorOwner>(), Arg.Any<CorrelationId>(), Arg.Any<CapturedError>())).Throw(new InvalidOperationException("The store is unavailable"));
        _services.AddSingleton(errors);
    }

    void Because()
    {
        _provider = _services.BuildServiceProvider();
        _error = Catch.Exception(() => Fail(_provider));
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_not_fail() => _error.ShouldBeNull();
    [Fact] void should_leave_the_failure_to_arc() => _mapped.ShouldBeFalse();
    [Fact] void should_not_give_a_result() => _result.ShouldBeNull();
}
#endif
