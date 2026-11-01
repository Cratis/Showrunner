// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;

namespace Cratis.Showrunner.Herald.for_EmailDelivery.when_delivering;

public class a_failure_that_names_a_secret : given.a_queued_email
{
    IReadOnlyList<object> _facts;
    string _everything;

    async Task Because()
    {
        _transport.Failure = new InvalidOperationException($"login failed for ledger with {Password} for grace@example.com");
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _transport.Failure = new EmailTransportException($"535 bad password {Password} for grace@example.com", permanent: true);
        _time.Advance(TimeSpan.FromHours(1));
        await _delivery.Deliver(_eventStore, Invitation, _id);
        _facts = await Facts();
        _everything = string.Join('\n', _facts.OfType<EmailAttemptFailed>().Select(fact => fact.Reason.Value).Concat(_facts.OfType<EmailFailed>().Select(fact => fact.Reason.Value)));
    }

    [Fact] void should_record_a_reason_for_each() => Assert.Equal(2, _everything.Split('\n').Length);
    [Fact] void should_keep_the_password_out_of_the_facts() => Assert.DoesNotContain(Password, _everything);
    [Fact] void should_keep_the_recipient_out_of_the_facts() => Assert.DoesNotContain("grace@example.com", _everything);
}
#endif
