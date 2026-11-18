// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventStoreSubscriptions;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorSubscriptions.when_naming_the_subscription;

public class for_an_operator_store : Specification
{
    EventStoreSubscriptionId _result;

    void Because() => _result = OperatorSubscriptions.SubscriptionFor("Direct");

    [Fact] void should_not_be_named_for_the_store_alone() => _result.Value.ShouldNotEqual("Direct");
    [Fact] void should_carry_the_box_office_prefix() => _result.Value.ShouldEqual("boxoffice-Direct");
}
#endif
