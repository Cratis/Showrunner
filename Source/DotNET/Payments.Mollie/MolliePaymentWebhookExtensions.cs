// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Payments.Mollie;

/// <summary>
/// Extension methods for mapping Mollie's webhook.
/// </summary>
public static class MolliePaymentWebhookExtensions
{
    /// <summary>
    /// The route Mollie reports a changed payment to, unless another is given.
    /// </summary>
    public const string DefaultRoute = "/webhooks/mollie";

    /// <summary>
    /// Maps the endpoint Mollie reports a changed payment to. The callback carries only the payment's id, so the payment is
    /// read back from Mollie before <see cref="IPaymentWebhookHandler"/> hears of it - a forged callback can name a payment
    /// but never change what it says. Anything after the route (<c>/webhooks/mollie/{scope}</c>) is handed on as the
    /// update's <see cref="PaymentUpdate.Scope"/>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map on.</param>
    /// <param name="route">The route to map.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    /// <remarks>
    /// The endpoint answers 404 while Mollie is not configured, 400 for a callback without an id, 502 when the payment
    /// cannot be read yet and 500 when the handler throws - Mollie delivers again on anything but 2xx.
    /// </remarks>
    public static IEndpointRouteBuilder MapMolliePaymentWebhook(this IEndpointRouteBuilder endpoints, string route = DefaultRoute)
    {
        var trimmed = route.TrimEnd('/');
        endpoints.MapPost(trimmed, (HttpContext http, CancellationToken cancellationToken) => Handle(http, null, cancellationToken)).AllowAnonymous();
        endpoints.MapPost(trimmed + "/{**scope}", (HttpContext http, string scope, CancellationToken cancellationToken) => Handle(http, scope, cancellationToken)).AllowAnonymous();
        return endpoints;
    }

    /// <summary>
    /// Handles one callback: reads the payment back from Mollie and hands it to the application's handler.
    /// </summary>
    /// <param name="http">The request.</param>
    /// <param name="scope">What followed the route, when anything did.</param>
    /// <param name="cancellationToken">Cancels the handling.</param>
    /// <returns>The answer Mollie gets.</returns>
    internal static async Task<IResult> Handle(HttpContext http, string? scope, CancellationToken cancellationToken)
    {
        var services = http.RequestServices;
        if (!services.GetRequiredService<IOptionsMonitor<MollieOptions>>().CurrentValue.IsConfigured)
        {
            return Results.NotFound();
        }

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(MolliePaymentWebhookExtensions));
        var form = await http.Request.ReadFormAsync(cancellationToken);
        var id = form["id"].ToString();
        if (string.IsNullOrWhiteSpace(id))
        {
            return Results.BadRequest();
        }

        Payment payment;
        try
        {
            payment = await services.GetRequiredService<IPaymentProvider>().GetPayment(id, cancellationToken);
        }
        catch (PaymentProviderException exception)
        {
            logger.CouldNotReadPayment(id, exception);
            return Results.StatusCode(StatusCodes.Status502BadGateway);
        }

        try
        {
            await services.GetRequiredService<IPaymentWebhookHandler>().Handle(new PaymentUpdate(payment, string.IsNullOrEmpty(scope) ? null : scope), cancellationToken);
            return Results.Ok();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.CouldNotHandlePayment(id, exception);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
