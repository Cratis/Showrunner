// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentProvider.when_reading_a_payment;

public class and_it_expired : given.a_mollie_provider
{
    Payment _payment;

    void Establish() => _handler.Response = """{"id":"tr_3","status":"expired","amount":{"currency":"EUR","value":"20.00"}}""";

    async Task Because() => _payment = await _provider.GetPayment("tr_3");

    [Fact] void should_be_expired() => Assert.Equal(PaymentStatus.Expired, _payment.Status);
    [Fact] void should_have_no_metadata() => Assert.Empty(_payment.Metadata);
}
#endif
