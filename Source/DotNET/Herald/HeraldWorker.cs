// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Delivers queued emails in the background, and retries the ones the mail server did not take when their time comes.
/// </summary>
/// <param name="delivery">The <see cref="EmailDelivery"/>.</param>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
/// <param name="time">The <see cref="TimeProvider"/>.</param>
/// <param name="logger">The logger.</param>
public class HeraldWorker(EmailDelivery delivery, IOptions<HeraldOptions> options, TimeProvider time, ILogger<HeraldWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await delivery.DeliverDue(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.RoundFailed(exception.GetType().Name);
            }

            try
            {
                await Task.Delay(options.Value.PollingInterval, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
