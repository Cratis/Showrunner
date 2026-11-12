// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Payments.Mollie;

/// <summary>
/// Extension methods for adding Mollie payments to an application.
/// </summary>
public static class MolliePaymentsServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mollie as the application's <see cref="IPaymentProvider"/>. Register an <see cref="IPaymentWebhookHandler"/>
    /// to hear about payments, and map the endpoint with <c>MapMolliePaymentWebhook</c>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configuration">The configuration holding the <see cref="MollieOptions.SectionName"/> section.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMolliePayments(this IServiceCollection services, IConfiguration configuration) =>
        services.AddMolliePayments(options => configuration.GetSection(MollieOptions.SectionName).Bind(options));

    /// <summary>
    /// Adds Mollie as the application's <see cref="IPaymentProvider"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures the <see cref="MollieOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMolliePayments(this IServiceCollection services, Action<MollieOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<IPaymentProvider, MolliePaymentProvider>(client => client.Timeout = TimeSpan.FromSeconds(30));
        return services;
    }
}
