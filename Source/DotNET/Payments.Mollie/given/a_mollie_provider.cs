// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Payments.Mollie.given;

public class a_mollie_provider : Specification
{
    protected RecordingHandler _handler;
    protected MollieOptions _options;
    protected MolliePaymentProvider _provider;

    void Establish()
    {
        _handler = new();
        _options = new() { ApiKey = "test_key" };
        _provider = new(new HttpClient(_handler), Monitor(_options));
    }

    protected static IOptionsMonitor<MollieOptions> Monitor(MollieOptions options)
    {
        var monitor = Substitute.For<IOptionsMonitor<MollieOptions>>();
        monitor.CurrentValue.Returns(options);
        return monitor;
    }

    public class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string Response { get; set; } = "{}";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(Status) { Content = new StringContent(Response, Encoding.UTF8, "application/json") };
        }
    }
}
#endif
