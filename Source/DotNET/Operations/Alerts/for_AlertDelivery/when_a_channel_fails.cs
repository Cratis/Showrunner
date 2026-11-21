// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Alerts.for_AlertDelivery;

public class when_a_channel_fails : Specification
{
    RecordingChannel _working;
    RecordingChannel _failing;
    DeliveryResult _result;

    void Establish()
    {
        _failing = new RecordingChannel("discord") { Fail = true };
        _working = new RecordingChannel("email");
    }

    async Task Because() => _result = await new AlertDelivery([_failing, _working], NullLogger<AlertDelivery>.Instance)
        .Deliver(new AlertNotification("shop", [new Finding("a", "a")], [], [], []), CancellationToken.None);

    [Fact] void should_still_deliver_to_the_other_channel() => _working.Delivered.Count.ShouldEqual(1);
    [Fact] void should_report_the_channel_that_delivered() => _result.Delivered.ShouldContainOnly("email");
    [Fact] void should_report_the_channel_that_failed_by_name_only() => _result.Failed.ShouldContainOnly("discord");
    [Fact] void should_count_as_delivered() => _result.AnyDelivered.ShouldBeTrue();
}
#endif
