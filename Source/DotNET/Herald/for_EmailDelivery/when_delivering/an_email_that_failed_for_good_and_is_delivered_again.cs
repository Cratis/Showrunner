// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class an_email_that_failed_for_good_and_is_delivered_again : given.a_queued_email
{
    IReadOnlyList<object> _facts;

    async Task Because()
    {
        await _eventStore.EventLog.Append(Invitation, new EmailFailed(_id, "invitation", EmailLanguage.NotSet, "refused", 1));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
    }

    [Fact] void should_send_nothing() => Assert.Equal(0, _transport.Offered);
    [Fact] void should_record_nothing_more() => Assert.Equal(2, _facts.Count);
}
#endif
