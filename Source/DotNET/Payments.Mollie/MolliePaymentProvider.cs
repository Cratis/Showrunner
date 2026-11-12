// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Payments.Mollie;

/// <summary>
/// The <see cref="IPaymentProvider"/> that talks to Mollie.
/// </summary>
/// <param name="client">The HTTP client used to reach Mollie.</param>
/// <param name="options">The Mollie options.</param>
public sealed class MolliePaymentProvider(HttpClient client, IOptionsMonitor<MollieOptions> options) : IPaymentProvider
{
    static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc/>
    public bool IsConfigured => options.CurrentValue.IsConfigured;

    /// <inheritdoc/>
    public async Task<CustomerId> CreateCustomer(NewCustomer customer, CancellationToken cancellationToken = default)
    {
        var dto = await Send<CustomerDto>(HttpMethod.Post, "customers", new CreateCustomerDto(customer.Name, customer.Email, customer.Metadata), nameof(CreateCustomer), cancellationToken);
        return dto.Id;
    }

    /// <inheritdoc/>
    public Task<Payment> CreateFirstPayment(PaymentRequest request, CancellationToken cancellationToken = default) =>
        CreatePayment(request, "first", cancellationToken);

    /// <inheritdoc/>
    public Task<Payment> CreateOneOffPayment(PaymentRequest request, CancellationToken cancellationToken = default) =>
        CreatePayment(request, "oneoff", cancellationToken);

    /// <inheritdoc/>
    public async Task<Payment> ChargeMandate(MandateCharge charge, CancellationToken cancellationToken = default)
    {
        var body = new CreatePaymentDto(ToDto(charge.Amount), charge.Description, null, charge.WebhookUrl, "recurring", charge.CustomerId, charge.Metadata);
        return ToPayment(await Send<PaymentDto>(HttpMethod.Post, "payments", body, nameof(ChargeMandate), cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<Payment> GetPayment(PaymentId payment, CancellationToken cancellationToken = default) =>
        ToPayment(await Send<PaymentDto>(HttpMethod.Get, $"payments/{Uri.EscapeDataString(payment)}", null, nameof(GetPayment), cancellationToken));

    /// <inheritdoc/>
    public Task Refund(PaymentId payment, PaymentAmount amount, CancellationToken cancellationToken = default) =>
        Send<JsonElement>(HttpMethod.Post, $"payments/{Uri.EscapeDataString(payment)}/refunds", new RefundDto(ToDto(amount)), nameof(Refund), cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RecurringSubscription>> GetSubscriptions(CustomerId customer, CancellationToken cancellationToken = default)
    {
        var dto = await Send<SubscriptionListDto>(HttpMethod.Get, $"customers/{Uri.EscapeDataString(customer)}/subscriptions", null, nameof(GetSubscriptions), cancellationToken);
        return [.. (dto.Embedded?.Subscriptions ?? []).Select(ToSubscription)];
    }

    /// <inheritdoc/>
    public async Task<RecurringSubscription> GetSubscription(CustomerId customer, SubscriptionId subscription, CancellationToken cancellationToken = default) =>
        ToSubscription(await Send<SubscriptionDto>(HttpMethod.Get, SubscriptionPath(customer, subscription), null, nameof(GetSubscription), cancellationToken));

    /// <inheritdoc/>
    public async Task<RecurringSubscription> CreateSubscription(SubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var body = new CreateSubscriptionDto(ToDto(request.Amount), ToInterval(request.Interval), request.Description, request.WebhookUrl, request.Metadata);
        return ToSubscription(await Send<SubscriptionDto>(HttpMethod.Post, $"customers/{Uri.EscapeDataString(request.CustomerId)}/subscriptions", body, nameof(CreateSubscription), cancellationToken));
    }

    /// <inheritdoc/>
    public Task UpdateSubscription(CustomerId customer, SubscriptionId subscription, PaymentAmount amount, string description, IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken = default) =>
        Send<SubscriptionDto>(HttpMethod.Patch, SubscriptionPath(customer, subscription), new UpdateSubscriptionDto(ToDto(amount), description, metadata), nameof(UpdateSubscription), cancellationToken);

    /// <inheritdoc/>
    public async Task CancelSubscription(CustomerId customer, SubscriptionId subscription, CancellationToken cancellationToken = default)
    {
        using var request = Request(HttpMethod.Delete, SubscriptionPath(customer, subscription), null);
        using var response = await Execute(request, nameof(CancelSubscription), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new PaymentProviderException(nameof(CancelSubscription), (int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
        }
    }

    static PaymentStatus ToStatus(string status) => status switch
    {
        "paid" => PaymentStatus.Paid,
        "failed" => PaymentStatus.Failed,
        "canceled" => PaymentStatus.Canceled,
        "expired" => PaymentStatus.Expired,
        _ => PaymentStatus.Open
    };

    static string SubscriptionPath(CustomerId customer, SubscriptionId subscription) =>
        $"customers/{Uri.EscapeDataString(customer)}/subscriptions/{Uri.EscapeDataString(subscription)}";

    static string ToInterval(PaymentInterval interval) => interval == PaymentInterval.Yearly ? "12 months" : "1 month";

    static AmountDto ToDto(PaymentAmount amount) => new(amount.Currency, amount.Value.ToString("0.00", CultureInfo.InvariantCulture));

    static PaymentSequence ToSequence(string? sequence) => sequence switch
    {
        "first" => PaymentSequence.First,
        "recurring" => PaymentSequence.Recurring,
        _ => PaymentSequence.OneOff
    };

    static SubscriptionStatus ToSubscriptionStatus(string status) => status switch
    {
        "active" => SubscriptionStatus.Active,
        "canceled" => SubscriptionStatus.Canceled,
        "suspended" => SubscriptionStatus.Suspended,
        _ => SubscriptionStatus.Other
    };

    static RecurringSubscription ToSubscription(SubscriptionDto dto) => new(dto.Id, ToSubscriptionStatus(dto.Status));

    static Payment ToPayment(PaymentDto dto) => new(
        dto.Id,
        ToStatus(dto.Status),
        new(decimal.Parse(dto.Amount?.Value ?? "0", CultureInfo.InvariantCulture), dto.Amount?.Currency ?? string.Empty),
        ToSequence(dto.SequenceType),
        dto.CustomerId is null ? null : new CustomerId(dto.CustomerId),
        dto.SubscriptionId is null ? null : new SubscriptionId(dto.SubscriptionId),
        dto.MandateId,
        dto.Links?.Checkout?.Href,
        dto.Metadata ?? []);

    async Task<Payment> CreatePayment(PaymentRequest request, string sequenceType, CancellationToken cancellationToken)
    {
        var body = new CreatePaymentDto(ToDto(request.Amount), request.Description, request.RedirectUrl, request.WebhookUrl, sequenceType, request.CustomerId?.Value, request.Metadata);
        return ToPayment(await Send<PaymentDto>(HttpMethod.Post, "payments", body, nameof(CreatePayment), cancellationToken));
    }

    HttpRequestMessage Request(HttpMethod method, string path, object? body)
    {
        var request = new HttpRequestMessage(method, $"{options.CurrentValue.ApiBaseUrl.TrimEnd('/')}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.CurrentValue.ApiKey);
        if (body is not null)
        {
            request.Content = System.Net.Http.Json.JsonContent.Create(body, body.GetType(), options: _json);
        }

        return request;
    }

    async Task<HttpResponseMessage> Execute(HttpRequestMessage request, string operation, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new PaymentProviderException(operation, 0, "Mollie is not configured: no API key is set");
        }

        try
        {
            return await client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new PaymentProviderException(operation, 0, exception.Message);
        }
    }

    async Task<TResponse> Send<TResponse>(HttpMethod method, string path, object? body, string operation, CancellationToken cancellationToken)
    {
        using var request = Request(method, path, body);
        using var response = await Execute(request, operation, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new PaymentProviderException(operation, (int)response.StatusCode, content);
        }

        return JsonSerializer.Deserialize<TResponse>(content, _json)
            ?? throw new PaymentProviderException(operation, (int)response.StatusCode, "empty response body");
    }

    sealed record AmountDto(string Currency, string Value);
    sealed record CreateCustomerDto(string Name, string? Email, object? Metadata);
    sealed record CustomerDto(string Id);
    sealed record CreatePaymentDto(AmountDto Amount, string Description, string? RedirectUrl, string? WebhookUrl, string SequenceType, string? CustomerId, object? Metadata);
    sealed record CreateSubscriptionDto(AmountDto Amount, string Interval, string Description, string? WebhookUrl, object? Metadata);
    sealed record UpdateSubscriptionDto(AmountDto Amount, string Description, object? Metadata);
    sealed record RefundDto(AmountDto Amount);
    sealed record LinkDto(string Href);
    sealed record LinksDto(LinkDto? Checkout);
    sealed record PaymentDto(
        string Id,
        string Status,
        AmountDto? Amount,
        string? CustomerId,
        string? SubscriptionId,
        string? MandateId,
        string? SequenceType,
        Dictionary<string, string>? Metadata,
        [property: JsonPropertyName("_links")] LinksDto? Links);
    sealed record SubscriptionDto(string Id, string Status);
    sealed record SubscriptionListDto([property: JsonPropertyName("_embedded")] SubscriptionListEmbeddedDto? Embedded);
    sealed record SubscriptionListEmbeddedDto(List<SubscriptionDto>? Subscriptions);
}
