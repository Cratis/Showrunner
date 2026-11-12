// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentProvider.when_reading_a_payment;

public class and_it_is_paid : given.a_mollie_provider
{
    Payment _payment;

    void Establish() => _handler.Response = """{"id":"tr_2","status":"paid","amount":{"currency":"EUR","value":"199.00"},"sequenceType":"recurring","subscriptionId":"sub_1","metadata":{"tenant":"acme"}}""";

    async Task Because() => _payment = await _provider.GetPayment("tr_2");

    [Fact] void should_read_the_payment_by_id() => Assert.EndsWith("/payments/tr_2", _handler.Request!.RequestUri!.ToString());
    [Fact] void should_be_paid() => Assert.Equal(PaymentStatus.Paid, _payment.Status);
    [Fact] void should_carry_the_amount() => Assert.Equal(new PaymentAmount(199m, "EUR"), _payment.Amount);
    [Fact] void should_carry_the_subscription() => Assert.Equal(new SubscriptionId("sub_1"), _payment.SubscriptionId);
    [Fact] void should_be_recurring() => Assert.Equal(PaymentSequence.Recurring, _payment.Sequence);
    [Fact] void should_carry_the_metadata() => Assert.Equal("acme", _payment.Metadata["tenant"]);
}
#endif
