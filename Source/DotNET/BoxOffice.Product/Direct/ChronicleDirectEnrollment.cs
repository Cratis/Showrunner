// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.Federation;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Redeems an enrollment code on the product's own Chronicle kernel through <see cref="IChronicleClient.Federation"/>.
/// The kernel keeps the certificate it is issued, renews it and dials the authority itself, so the product stores no
/// credentials and the only thing it records is the link's state.
/// </summary>
/// <remarks>
/// <para>
/// The Chronicle principal of the application must be allowed to redeem: it must be an administrator, or be listed in the
/// kernel setting <c>Federation:Enrollment:Applications</c>.
/// </para>
/// <para>
/// The kernel answers every failure to redeem the same way, whether the code is wrong, expired or already used, or the
/// authority is unreachable, wrong or refusing, so that it does not tell what an address can reach. An answer from the kernel
/// is therefore <see cref="DirectRedemption.Refused"/>, and <see cref="DirectRedemption.Unreachable"/> means the
/// kernel itself could not be reached (a transport failure, not an answer). The code is never logged and is never part of an exception message this class makes.
/// </para>
/// <para>
/// <see cref="Verify"/> reads the kernel's enrollment status and reports the link as healthy only when the kernel says it is
/// <see cref="FederationEnrollmentStatus.Connected"/> and has a <see cref="FederationEnrollmentStatus.LastExchange"/>, which
/// is proof of a successful exchange over the link. A kernel that has not exchanged anything yet is given
/// <see cref="ConnectionPatience"/> to make its first exchange; its <see cref="FederationEnrollmentStatus.LastError"/> is logged
/// (the kernel scrubs it of secrets) but never returned to the caller.
/// </para>
/// </remarks>
/// <param name="client">The <see cref="IChronicleClient"/> of the product.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class ChronicleDirectEnrollment(IChronicleClient client, ILogger<ChronicleDirectEnrollment> logger) : IDirectEnrollment
{
    const string CodePrefix = "cfe1.";
    const int MaximumReasonLength = 200;
    static readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Gets how long to wait for the first exchange of a kernel that is enrolled but has not exchanged anything yet.
    /// </summary>
    public TimeSpan ConnectionPatience { get; init; } = TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public async Task<DirectRedemptionResult> Redeem(DirectEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var status = await TryGetStatus(cancellationToken);
        if (status is { Enrolled: true, Expired: false })
        {
            // The kernel is enrolled already, perhaps from a code file or an earlier attempt: report it as redeemed so
            // the link can go on to connect, and leave the code unspent.
            logger.AlreadyEnrolled();
            return OnAllowedHost(request, status.Endpoint) ? new(DirectRedemption.Redeemed, status.Endpoint) : new(DirectRedemption.EndpointNotAllowed);
        }

        if (!EndpointAllowedBeforeRedeeming(request))
        {
            return new(DirectRedemption.EndpointNotAllowed);
        }

        try
        {
            var redemption = await client.Federation.Redeem(request.Code, cancellationToken);
            if (!OnAllowedHost(request, redemption.Endpoint))
            {
                // Only reachable when the kernel could not tell the code's endpoint beforehand; the kernel is
                // enrolled by now, and the next attempt meets the same check through its status.
                logger.EndpointNotAllowedAfterRedeeming();
                return new(DirectRedemption.EndpointNotAllowed);
            }

            return new(DirectRedemption.Redeemed, redemption.Endpoint);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (KernelNotReached(exception))
        {
            logger.RedemptionNotReachable(exception.GetType().Name);
            return new(DirectRedemption.Unreachable);
        }
        catch (Exception exception)
        {
            // The kernel answered, and a failed command is thrown whatever the reason, so this is the uniform refusal.
            logger.RedemptionRefused(exception.GetType().Name);
            return new(DirectRedemption.Refused);
        }
    }

    /// <inheritdoc/>
    public async Task<DirectLinkHealth> Verify(CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + ConnectionPatience;
        while (true)
        {
            var status = await TryGetStatus(cancellationToken);
            if (status is null)
            {
                return DirectLinkHealth.Unreachable;
            }

            if (!status.Enrolled || status.Expired)
            {
                logger.EnrollmentLost(status.Enrolled, status.Expired);
                return DirectLinkHealth.Unrecoverable;
            }

            if (status.Connected && status.LastExchange is not null)
            {
                return DirectLinkHealth.Healthy;
            }

            // Only a kernel that has never exchanged is given time: it dials after it is enrolled. A kernel that has
            // exchanged before and is not connected now has lost the link, and waiting would only slow the caller.
            if (status.LastExchange is not null || DateTimeOffset.UtcNow >= deadline)
            {
                logger.NotConnected(Reason(status.LastError));
                return DirectLinkHealth.Unreachable;
            }

            await Task.Delay(_pollInterval, cancellationToken);
        }
    }

    static string Reason(string lastError)
    {
        var reason = lastError.ReplaceLineEndings(" ").Trim();
        if (reason.Length == 0)
        {
            return "none reported";
        }

        return reason.Length > MaximumReasonLength ? reason[..MaximumReasonLength] : reason;
    }

    static bool KernelNotReached(Exception exception) =>
        exception is HttpRequestException or IOException or TimeoutException or OperationCanceledException
            or RpcException { StatusCode: StatusCode.Unavailable or StatusCode.DeadlineExceeded };

    static bool OnAllowedHost(DirectEnrollmentRequest request, string endpoint) =>
        request.ExpectedHost.Length == 0 ||
        (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            string.Equals(uri.IdnHost, request.ExpectedHost, StringComparison.OrdinalIgnoreCase));

    static Uri? EndpointOf(string code)
    {
        // The code is one string carrying the endpoint of the authority. Reading it here lets the product refuse an
        // endpoint before the kernel connects to it; the kernel still checks what it connects to.
        var trimmed = code.Trim();
        if (!trimmed.StartsWith(CodePrefix, StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            var base64 = trimmed[CodePrefix.Length..].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
            using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
            return document.RootElement.TryGetProperty("e", out var endpoint) &&
                Uri.TryCreate(endpoint.GetString(), UriKind.Absolute, out var uri)
                    ? uri
                    : null;
        }
        catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException or ArgumentException)
        {
            return null;
        }
    }

    bool EndpointAllowedBeforeRedeeming(DirectEnrollmentRequest request)
    {
        var typed = string.IsNullOrWhiteSpace(request.Endpoint) ? null : request.Endpoint.Trim();
        if (request.ExpectedHost.Length == 0 && typed is null)
        {
            // Nothing to enforce here; the kernel's own endpoint policy applies.
            return true;
        }

        var carried = EndpointOf(request.Code);
        if (carried is null)
        {
            // An endpoint that cannot be read cannot be checked, so it is refused rather than trusted.
            logger.EndpointNotReadable();
            return false;
        }

        // The kernel redeems against the endpoint the code carries; it takes no other. An endpoint typed by hand can
        // therefore only confirm it, never replace it.
        return OnAllowedHost(request, carried.ToString()) &&
            (typed is null || (Uri.TryCreate(typed, UriKind.Absolute, out var typedUri) && typedUri.Authority.Equals(carried.Authority, StringComparison.OrdinalIgnoreCase)));
    }

    async Task<FederationEnrollmentStatus?> TryGetStatus(CancellationToken cancellationToken)
    {
        try
        {
            return await client.Federation.GetStatus(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.StatusNotAvailable(exception.GetType().Name);
            return null;
        }
    }
}
