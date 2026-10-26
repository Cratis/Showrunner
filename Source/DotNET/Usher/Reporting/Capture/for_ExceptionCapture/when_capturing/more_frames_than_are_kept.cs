// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.when_capturing;

public class more_frames_than_are_kept : Specification
{
    CapturedError _oneFrame;
    CapturedError _noFrames;

    void Because()
    {
        _oneFrame = ExceptionCapture.Capture(Thrower.Thrown(), SliceLocation.NotSet, ApplicationVersion.NotSet, 1);
        _noFrames = ExceptionCapture.Capture(Thrower.Thrown(), SliceLocation.NotSet, ApplicationVersion.NotSet, 0);
    }

    [Fact] void should_keep_the_top_frames_only() => _oneFrame.StackFrames.Select(_ => _.Method.Value).ShouldEqual([$"{typeof(Thrower).FullName}.Throw"]);
    [Fact] void should_keep_none_when_none_are_kept() => _noFrames.StackFrames.ShouldBeEmpty();
}
#endif
