// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// The state machine of the link to Chronicle: not configured, redeemed, closed. It is the one place that redeems a
/// code, whether it comes from the page or from configuration.
/// </summary>
/// <remarks>
/// A code is only redeemed once. When redeeming succeeds but connecting fails, the credentials stay stored and the next
/// attempt connects with them, needing no code. Once the first exchange succeeds the link is closed for good, and it only
/// opens again when re-enrollment is allowed and the link can never work again.
/// </remarks>
/// <param name="enrollment">The <see cref="IDirectEnrollment"/> port.</param>
/// <param name="record">The <see cref="IDirectLinkRecord"/> that keeps the state.</param>
/// <param name="limiter">The <see cref="AttemptLimiter"/>.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class DirectConnection(
    IDirectEnrollment enrollment,
    IDirectLinkRecord record,
    AttemptLimiter limiter,
    IOptions<BoxOfficeProductOptions> options,
    ILogger<DirectConnection> logger) : IDisposable
{
    /// <summary>
    /// The longest enrollment code that is considered.
    /// </summary>
    public const int MaximumCodeLength = 4096;

    readonly SemaphoreSlim _gate = new(1, 1);

    DirectConnectionOptions Settings => options.Value.Direct;

    /// <inheritdoc/>
    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the state of the link, which opens it again when that is allowed and the link can never work again.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DirectLinkState"/>.</returns>
    public async Task<DirectLinkState> Current(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await Read(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Makes an attempt to link the product: redeems the code when none has been redeemed, then connects.
    /// </summary>
    /// <param name="code">The enrollment code; it is not needed, and ignored, once one has been redeemed.</param>
    /// <param name="endpoint">A manually typed endpoint, or empty to use the one the code carries.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DirectAttemptResult"/>.</returns>
    public async Task<DirectAttemptResult> Attempt(string? code, string? endpoint, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var state = await Read(cancellationToken);
            if (state.Stage == DirectLinkStage.Closed)
            {
                return DirectAttemptResult.Closed;
            }

            if (state.Stage == DirectLinkStage.NotConfigured && (string.IsNullOrWhiteSpace(code) || code.Length > MaximumCodeLength))
            {
                return DirectAttemptResult.NoCode;
            }

            if (state.Stage == DirectLinkStage.NotConfigured && !EndpointAllowed(endpoint))
            {
                logger.EndpointRefused();
                return DirectAttemptResult.EndpointRefused;
            }

            if (!limiter.TryAcquire())
            {
                logger.TooManyAttempts();
                return DirectAttemptResult.TooManyAttempts;
            }

            if (state.Stage == DirectLinkStage.NotConfigured)
            {
                var refused = await Redeem(code!.Trim(), endpoint, cancellationToken);
                if (refused is not null)
                {
                    return refused.Value;
                }
            }

            return await Connect(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    async Task<DirectLinkState> Read(CancellationToken cancellationToken)
    {
        var state = await record.Read(cancellationToken);
        if (state.Stage == DirectLinkStage.NotConfigured || !Settings.AllowReenrollment)
        {
            return state;
        }

        if (await enrollment.Verify(cancellationToken) != DirectLinkHealth.Unrecoverable)
        {
            return state;
        }

        await record.Reopened(cancellationToken);
        logger.Reopened();
        return await record.Read(cancellationToken);
    }

    async Task<DirectAttemptResult?> Redeem(string code, string? endpoint, CancellationToken cancellationToken)
    {
        var typed = string.IsNullOrWhiteSpace(endpoint) ? null : endpoint.Trim();
        var result = await enrollment.Redeem(new(code, typed, Settings.ExpectedEndpointHost), cancellationToken);
        if (result.Outcome == DirectRedemption.Redeemed)
        {
            await record.Redeemed(result.Endpoint, cancellationToken);
            logger.Redeemed();
            return null;
        }

        logger.NotRedeemed(result.Outcome);
        return result.Outcome switch
        {
            DirectRedemption.Unreachable => DirectAttemptResult.Unreachable,
            DirectRedemption.EndpointNotAllowed => DirectAttemptResult.EndpointRefused,
            _ => DirectAttemptResult.CodeRefused
        };
    }

    async Task<DirectAttemptResult> Connect(CancellationToken cancellationToken)
    {
        if (await enrollment.Verify(cancellationToken) != DirectLinkHealth.Healthy)
        {
            logger.ConnectionFailed();
            return DirectAttemptResult.ConnectionFailed;
        }

        await record.Verified(cancellationToken);
        logger.Connected();
        return DirectAttemptResult.Connected;
    }

    bool EndpointAllowed(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return true;
        }

        var pinned = Settings.ExpectedEndpointHost;
        return pinned.Length > 0 &&
            Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            uri.UserInfo.Length == 0 &&
            string.Equals(uri.IdnHost, pinned, StringComparison.OrdinalIgnoreCase);
    }
}
