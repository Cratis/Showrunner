// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class a_sender_without_a_reply_to_address : given.a_queued_email
{
    async Task Because()
    {
        _options.Sender.ReplyTo = string.Empty;
        await _delivery.Deliver(_eventStore, Invitation, _id);
    }

    [Fact] void should_ask_for_no_reply_address() => Assert.Null(_transport.Accepted.Single().ReplyTo);
}
#endif
