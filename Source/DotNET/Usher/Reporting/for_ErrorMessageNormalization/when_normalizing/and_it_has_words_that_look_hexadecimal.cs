// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_words_that_look_hexadecimal : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("Deadbeef cafe facade accepted");

    [Fact] void should_keep_words_without_digits() => _normalized.ShouldEqual("Deadbeef cafe facade accepted");
}
#endif
