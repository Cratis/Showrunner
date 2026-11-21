// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Alerts.for_AlertDelivery;

public class when_no_channel_is_configured : Specification
{
    DeliveryResult _result;

    async Task Because() => _result = await new AlertDelivery([], NullLogger<AlertDelivery>.Instance)
        .Deliver(new AlertNotification("shop", [new Finding("a", "a")], [], [], []), CancellationToken.None);

    [Fact] void should_not_count_as_delivered() => _result.AnyDelivered.ShouldBeFalse();
    [Fact] void should_not_name_a_failure() => _result.Failed.ShouldBeEmpty();
}
#endif
