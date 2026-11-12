// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;

namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentProvider.when_mollie_refuses;

public class and_a_refund_is_made : given.a_mollie_provider
{
    PaymentProviderException? _exception;

    void Establish()
    {
        _handler.Status = HttpStatusCode.UnprocessableEntity;
        _handler.Response = """{"detail":"The amount is higher than the maximum refundable amount"}""";
    }

    async Task Because() => _exception = await Assert.ThrowsAsync<PaymentProviderException>(() => _provider.Refund("tr_1", new(500m, "EUR")));

    [Fact] void should_carry_the_status() => Assert.Equal(422, _exception!.StatusCode);
    [Fact] void should_carry_what_mollie_said() => Assert.Contains("maximum refundable", _exception!.Body);
    [Fact] void should_name_the_operation() => Assert.Equal("Refund", _exception!.Operation);
}
#endif
