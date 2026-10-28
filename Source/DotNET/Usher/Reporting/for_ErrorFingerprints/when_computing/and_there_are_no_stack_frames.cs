// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

public class and_there_are_no_stack_frames : an_error
{
    ErrorFingerprint _withoutFrames;
    ErrorFingerprint _withAnEmptyTopFrame;
    ErrorFingerprint _withFrames;

    void Because()
    {
        _withoutFrames = FingerprintOf("Invoice 42 was not found after 3 attempts", []);
        _withAnEmptyTopFrame = FingerprintOf("Invoice 42 was not found after 3 attempts", [new(StackFrameMethod.NotSet, StackFrameFile.NotSet, StackFrameLine.NotSet)]);
        _withFrames = FingerprintOf("Invoice 42 was not found after 3 attempts");
    }

    [Fact] void should_be_built_from_the_rest_of_the_error() => _withoutFrames.Value.ShouldEqual("e31d0d9b957168e69404000027943ecf389866a6c080c9b93473e3280e601337");
    [Fact] void should_treat_an_empty_top_frame_as_no_frames() => _withAnEmptyTopFrame.ShouldEqual(_withoutFrames);
    [Fact] void should_differ_from_the_error_with_frames() => _withFrames.ShouldNotEqual(_withoutFrames);
}
#endif
