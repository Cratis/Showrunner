// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Contracts.for_IssueId;

/// <summary>
/// The application and every desk address the same issue by its event source id, so the string form must be the
/// lowercase, hyphenated <c>D</c> representation on both sides.
/// </summary>
public class when_converting_to_an_event_source_id : Specification
{
    static readonly Guid _value = new(0x8F14E45F, 0xCEEA, 0x467A, 0x95, 0x75, 0x7C, 0x0A, 0x9B, 0x6B, 0x0C, 0x2D);
    EventSourceId _result = EventSourceId.Unspecified;

    void Because() => _result = new IssueId(_value);

    [Fact] void should_use_the_lowercase_hyphenated_representation() => Assert.Equal("8f14e45f-ceea-467a-9575-7c0a9b6b0c2d", _result.Value);
}
#endif
