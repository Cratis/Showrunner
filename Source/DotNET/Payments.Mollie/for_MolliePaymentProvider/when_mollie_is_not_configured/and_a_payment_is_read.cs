// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentProvider.when_mollie_is_not_configured;

public class and_a_payment_is_read : given.a_mollie_provider
{
    Exception? _exception;

    void Establish() => _options.ApiKey = string.Empty;

    async Task Because() => _exception = await Record.ExceptionAsync(() => _provider.GetPayment("tr_1"));

    [Fact] void should_not_be_configured() => Assert.False(_provider.IsConfigured);
    [Fact] void should_refuse() => Assert.IsType<PaymentProviderException>(_exception);
    [Fact] void should_not_call_mollie() => Assert.Null(_handler.Request);
}
#endif
