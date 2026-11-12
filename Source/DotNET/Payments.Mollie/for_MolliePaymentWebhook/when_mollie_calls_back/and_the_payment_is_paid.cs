// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Payments.Mollie.for_MolliePaymentWebhook.when_mollie_calls_back;

public class and_the_payment_is_paid : Specification
{
    IPaymentProvider _provider;
    IPaymentWebhookHandler _handler;
    PaymentUpdate? _update;
    int _status;

    void Establish()
    {
        var payment = new Payment("tr_9", PaymentStatus.Paid, new(20m, "EUR"), PaymentSequence.First, null, null, null, null, new Dictionary<string, string>());
        _provider = Substitute.For<IPaymentProvider>();
        _provider.GetPayment("tr_9", Arg.Any<CancellationToken>()).Returns(payment);
        _handler = Substitute.For<IPaymentWebhookHandler>();
        _handler.Handle(Arg.Do<PaymentUpdate>(update => _update = update), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    async Task Because()
    {
        var services = new ServiceCollection()
            .AddSingleton(_provider)
            .AddSingleton(_handler)
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .AddSingleton<IOptionsMonitor<MollieOptions>>(Options("key"))
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Method = "POST";
        http.Request.ContentType = "application/x-www-form-urlencoded";
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("id=tr_9"));
        http.Response.Body = new MemoryStream();

        var result = await MolliePaymentWebhookExtensions.Handle(http, "11111111", CancellationToken.None);
        await result.ExecuteAsync(http);
        _status = http.Response.StatusCode;
    }

    static IOptionsMonitor<MollieOptions> Options(string key)
    {
        var monitor = Substitute.For<IOptionsMonitor<MollieOptions>>();
        monitor.CurrentValue.Returns(new MollieOptions { ApiKey = key });
        return monitor;
    }

    [Fact] void should_acknowledge() => Assert.Equal(200, _status);
    [Fact] void should_hand_on_the_payment_read_back_from_mollie() => Assert.Equal(PaymentStatus.Paid, _update!.Payment.Status);
    [Fact] void should_hand_on_what_followed_the_route() => Assert.Equal("11111111", _update!.Scope);
}
#endif
