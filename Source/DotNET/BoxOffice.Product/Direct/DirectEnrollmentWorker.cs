// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Redeems the enrollment code from configuration at startup, so a deployment can link the product without the page. It
/// does nothing once the link is established.
/// </summary>
/// <param name="services">The <see cref="IServiceProvider"/> that finds the <see cref="DirectConnection"/> only when it is needed, so a product that does not enable Direct need not register an enrollment.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class DirectEnrollmentWorker(IServiceProvider services, IOptions<BoxOfficeProductOptions> options, ILogger<DirectEnrollmentWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value.Direct;
        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.EnrollmentCode))
        {
            return;
        }

        try
        {
            var result = await ((DirectConnection)services.GetService(typeof(DirectConnection))!).Attempt(settings.EnrollmentCode, null, stoppingToken);
            logger.ConfiguredAttempt(result);
        }
        catch (OperationCanceledException)
        {
            // Stopping before it finished is not a failure.
        }
        catch (Exception exception)
        {
            logger.ConfiguredAttemptFailed(exception.GetType().Name);
        }
    }
}
