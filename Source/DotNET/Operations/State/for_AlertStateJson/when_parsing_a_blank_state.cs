// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.State.for_AlertStateJson;

public class when_parsing_a_blank_state : Specification
{
    AlertStateSnapshot _fromNull;
    AlertStateSnapshot _fromEmptyObject;

    void Because()
    {
        _fromNull = AlertStateJson.Parse(null);
        _fromEmptyObject = AlertStateJson.Parse("{}");
    }

    [Fact] void should_be_empty_for_nothing() => _fromNull.ShouldEqual(AlertStateSnapshot.Empty);
    [Fact] void should_have_no_alerts_for_an_empty_object() => _fromEmptyObject.LastAlerted.ShouldBeEmpty();
    [Fact] void should_have_no_last_check_for_an_empty_object() => _fromEmptyObject.LastCheck.ShouldBeNull();
}
#endif
