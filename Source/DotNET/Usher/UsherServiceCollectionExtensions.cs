// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc;
using Cratis.Showrunner.Usher;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Receiving;
using Cratis.Showrunner.Usher.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture;
using Cratis.Showrunner.Usher.Tenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Usher's services to a service collection.
/// </summary>
public static class UsherServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services Usher's commands and queries use, and validates the options when the host starts.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures Usher, at least <see cref="UsherOptions.ApplicationName"/>.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddUsher(this IServiceCollection services, Action<UsherOptions> configure)
    {
        services
            .AddOptions<UsherOptions>()
            .Configure(configure)
            .Validate(options => options.ApplicationName != ApplicationName.NotSet, "Usher needs an application name: set UsherOptions.ApplicationName in AddUsher.")
            .Validate(options => options.DeskStores.Any(), "Usher needs at least one desk store: set UsherOptions.DeskStores in AddUsher.")
            .Validate(options => options.ErrorCapture.Retention > TimeSpan.Zero, "Usher keeps captured errors for a while: set UsherOptions.ErrorCapture.Retention to more than zero.")
            .Validate(options => options.ErrorCapture.Capacity >= 0, "Usher keeps a number of captured errors: UsherOptions.ErrorCapture.Capacity cannot be negative.")
            .Validate(options => options.ErrorCapture.MaximumStackFrames >= 0, "Usher keeps a number of stack frames: UsherOptions.ErrorCapture.MaximumStackFrames cannot be negative.")
            .ValidateOnStart();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ArcOptions>, OnBehalfConfiguration>());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<ISignedInReporter, SignedInReporter>();
        services.TryAddSingleton<IComputeErrorFingerprints, ErrorFingerprints>();
        services.TryAddSingleton<ICapturedErrors, CapturedErrors>();
        services.TryAddTransient<ICurrentErrorOwner, CurrentErrorOwner>();
        services.TryAddTransient<CaptureFailedCommands>();
        services.TryAddSingleton<TenantEventStores>();
        services.TryAddSingleton<ITenantEventStores>(serviceProvider => serviceProvider.GetRequiredService<TenantEventStores>());
        services.TryAddSingleton<DeskEventTranslator>();
        services.TryAddSingleton<DeskSubscriptions>();
        services.TryAddSingleton<GrantLapsing>();
        services.TryAddSingleton<SupportSessions>();
        services.TryAddSingleton<OnBehalfTokenReplays>();
        services.TryAddSingleton<OnBehalfTokens>();
        services.TryAddTransient<IRepresentedPrincipals, RepresentedPrincipals>();
        services.TryAddTransient<OnBehalfSessionCalls>();
        services.AddHttpContextAccessor();
        services.TryAddTransient<IOnBehalfForward, OnBehalfForward>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, OnBehalfStartupFilter>());
        services.AddHostedService<UsherWorker>();
        return services;
    }
}
