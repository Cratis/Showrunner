// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfGrantAnswer;

public class when_serializing_a_live_answer : Specification
{
    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    static readonly OnBehalfClientPrincipal _principal = new("entra", "reporter", "Ada Lovelace", [], []);

    string _utc;
    string _withOffset;

    void Because()
    {
        _utc = ValidUntilOf(new OnBehalfGrantAnswer(true, new DateTimeOffset(2026, 10, 26, 13, 0, 0, TimeSpan.Zero), _principal));
        _withOffset = ValidUntilOf(new OnBehalfGrantAnswer(true, new DateTimeOffset(2026, 10, 26, 15, 0, 0, TimeSpan.FromHours(2)), _principal));
    }

    [Fact] void should_state_the_offset_of_a_utc_time() => _utc.ShouldEqual("2026-10-26T13:00:00+00:00");
    [Fact] void should_state_any_other_offset() => _withOffset.ShouldEqual("2026-10-26T15:00:00+02:00");

    static string ValidUntilOf(OnBehalfGrantAnswer answer) =>
        JsonDocument.Parse(JsonSerializer.Serialize(answer, _serializerOptions)).RootElement.GetProperty("validUntil").GetString()!;
}
#endif
