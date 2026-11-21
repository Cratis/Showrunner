// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Alerts.for_AlertDelivery;

public class when_every_channel_fails : Specification
{
    DeliveryResult _result;

    async Task Because() => _result = await new AlertDelivery([new RecordingChannel("discord") { Fail = true }, new RecordingChannel("email") { Fail = true }], NullLogger<AlertDelivery>.Instance)
        .Deliver(new AlertNotification("shop", [new Finding("a", "a")], [], [], []), CancellationToken.None);

    [Fact] void should_not_count_as_delivered() => _result.AnyDelivered.ShouldBeFalse();
    [Fact] void should_name_both_failures() => _result.Failed.ShouldContainOnly("discord", "email");
}
#endif
