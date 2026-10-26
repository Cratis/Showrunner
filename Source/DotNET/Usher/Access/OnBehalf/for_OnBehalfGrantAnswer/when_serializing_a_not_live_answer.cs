// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfGrantAnswer;

public class when_serializing_a_not_live_answer : Specification
{
    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    string _json;

    void Because() => _json = JsonSerializer.Serialize(OnBehalfGrantAnswer.NotLive, _serializerOptions);

    [Fact] void should_only_say_it_is_not_live() => _json.ShouldEqual("{\"live\":false}");
}
#endif
