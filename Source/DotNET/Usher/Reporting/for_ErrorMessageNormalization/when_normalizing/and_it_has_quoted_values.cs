// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_quoted_values : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("The 'Email' field of customer \"C-17\" is invalid");

    [Fact] void should_keep_quoted_values_and_normalize_ids_within_them() => _normalized.ShouldEqual("The 'Email' field of customer \"C-<n>\" is invalid");
}
#endif
