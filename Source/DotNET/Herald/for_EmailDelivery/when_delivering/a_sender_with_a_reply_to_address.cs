// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class a_sender_with_a_reply_to_address : given.a_queued_email
{
    async Task Because()
    {
        _options.Sender.ReplyTo = "help@ledger.example.com";
        await _delivery.Deliver(_eventStore, Invitation, _id);
    }

    [Fact] void should_ask_for_replies_to_go_to_it() => Assert.Equal("help@ledger.example.com", _transport.Accepted.Single().ReplyTo?.Address);
}
#endif
