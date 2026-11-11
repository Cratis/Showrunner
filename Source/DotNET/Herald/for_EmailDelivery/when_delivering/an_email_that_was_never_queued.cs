// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_was_never_queued : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        await _delivery.Deliver(_eventStore, Invitation, EmailId.New());
        _facts = await Facts();
    }

    [Fact] void should_send_nothing() => Assert.Equal(0, _transport.Offered);
    [Fact] void should_record_nothing() => Assert.Single(_facts);
}
#endif
