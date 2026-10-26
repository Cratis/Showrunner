// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.when_capturing;

public class an_exception_that_was_never_thrown : Specification
{
    CapturedError _error;

    void Because() => _error = ExceptionCapture.Capture(new InvalidOperationException("Never thrown"), SliceLocation.NotSet, ApplicationVersion.NotSet, 50);

    [Fact] void should_have_no_stack_frames() => _error.StackFrames.ShouldBeEmpty();
    [Fact] void should_still_have_the_message() => _error.ErrorMessage.Value.ShouldEqual("Never thrown");
}
#endif
