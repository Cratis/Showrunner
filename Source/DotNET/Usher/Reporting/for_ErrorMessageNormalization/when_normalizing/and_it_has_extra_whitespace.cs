// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_extra_whitespace : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("  Something\n\tfailed   badly  ");

    [Fact] void should_collapse_whitespace_and_trim() => _normalized.ShouldEqual("Something failed badly");
}
#endif
