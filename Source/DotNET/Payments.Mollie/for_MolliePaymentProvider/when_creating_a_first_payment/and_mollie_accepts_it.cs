// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentProvider.when_creating_a_first_payment;

public class and_mollie_accepts_it : given.a_mollie_provider
{
    Payment _payment;

    void Establish() => _handler.Response = """{"id":"tr_1","status":"open","amount":{"currency":"EUR","value":"20.00"},"sequenceType":"first","customerId":"cst_1","metadata":{"tenant":"acme"},"_links":{"checkout":{"href":"https://pay.example/tr_1"}}}""";

    async Task Because() => _payment = await _provider.CreateFirstPayment(
        new(new(20m, "EUR"), "Solo", "https://app/return", "https://app/webhooks/mollie", new Dictionary<string, string> { ["tenant"] = "acme" }, "cst_1"));

    [Fact] void should_post_to_payments() => Assert.Equal("https://api.mollie.com/v2/payments", _handler.Request!.RequestUri!.ToString());
    [Fact] void should_authenticate_with_the_api_key() => Assert.Equal("test_key", _handler.Request!.Headers.Authorization!.Parameter);
    [Fact] void should_ask_for_a_first_payment() => Assert.Contains("\"sequenceType\":\"first\"", _handler.RequestBody);
    [Fact] void should_send_the_amount_with_two_decimals() => Assert.Contains("\"value\":\"20.00\"", _handler.RequestBody);
    [Fact] void should_send_the_customer() => Assert.Contains("\"customerId\":\"cst_1\"", _handler.RequestBody);
    [Fact] void should_return_the_checkout() => Assert.Equal("https://pay.example/tr_1", _payment.CheckoutUrl);
    [Fact] void should_return_the_payment_open() => Assert.Equal(PaymentStatus.Open, _payment.Status);
    [Fact] void should_return_the_sequence() => Assert.Equal(PaymentSequence.First, _payment.Sequence);
}
#endif
