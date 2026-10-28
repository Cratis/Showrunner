// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.when_capturing;

public class a_wrapped_exception : Specification
{
    CapturedError _fromReflection;
    CapturedError _fromATask;

    void Because()
    {
        _fromReflection = ExceptionCapture.Capture(new TargetInvocationException(Thrower.Thrown()), SliceLocation.NotSet, ApplicationVersion.NotSet, 50);
        _fromATask = ExceptionCapture.Capture(new AggregateException(new TargetInvocationException(Thrower.Thrown())), SliceLocation.NotSet, ApplicationVersion.NotSet, 50);
    }

    [Fact] void should_capture_the_exception_reflection_wrapped() => _fromReflection.ErrorType.Value.ShouldEqual("System.InvalidOperationException");
    [Fact] void should_capture_the_frames_of_the_exception_reflection_wrapped() => _fromReflection.StackFrames.First().Method.Value.ShouldEqual($"{typeof(Thrower).FullName}.Throw");
    [Fact] void should_capture_the_exception_a_task_wrapped() => _fromATask.ErrorMessage.Value.ShouldEqual(Thrower.Message);
}
#endif
