// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_hexadecimal_ids : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("Document 507f1f77bcf86cd799439011 at 0x7ffe4a2c is locked");

    [Fact] void should_replace_each_hexadecimal_id_with_an_id_placeholder() => _normalized.ShouldEqual("Document <id> at <id> is locked");
}
#endif
