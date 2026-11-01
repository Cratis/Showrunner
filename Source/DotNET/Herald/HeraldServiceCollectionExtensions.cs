// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald;
using Cratis.Showrunner.Herald.Rendering;
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MimeKit;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Herald's services to a service collection.
/// </summary>
public static class HeraldServiceCollectionExtensions
{
    /// <summary>
    /// Adds Herald: <see cref="IEmails"/> to send with, and the background work that delivers and retries. The options are
    /// validated when the host starts.
    /// </summary>
    /// <remarks>
    /// Herald records its facts in the application's Chronicle event store, so the application also calls
    /// <c>AddCratisChronicle</c> (or <c>AddCratis</c>). Register an <see cref="IEmailTransport"/> before this call to send
    /// through something other than the SMTP server or folder <see cref="HeraldOptions"/> names.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="configure">Configures Herald: the templates folder, the sender and the mail server at least.</param>
    /// <returns>The service collection for continuation.</returns>
    public static IServiceCollection AddHerald(this IServiceCollection services, Action<HeraldOptions> configure)
    {
        services
            .AddOptions<HeraldOptions>()
            .Configure(configure)
            .Validate(options => Directory.Exists(options.TemplatesFolder), "Herald needs the folder with the templates: set HeraldOptions.TemplatesFolder in AddHerald to a folder that exists.")
            .Validate(options => options.ImagesFolder.Length == 0 || Directory.Exists(options.ImagesFolder), "Herald's images folder does not exist: set HeraldOptions.ImagesFolder to a folder that exists, or leave it unset when no template has an image.")
            .Validate(options => MailboxAddress.TryParse(options.Sender.Address, out _), "Herald needs the address emails come from: set HeraldOptions.Sender.Address in AddHerald.")
            .Validate(options => options.Sender.ReplyTo.Length == 0 || MailboxAddress.TryParse(options.Sender.ReplyTo, out _), "HeraldOptions.Sender.ReplyTo is not an email address.")
            .Validate(options => options.Transport != EmailTransportKind.Smtp || (options.Smtp.Host.Length > 0 && options.Smtp.Port is > 0 and <= 65535), "Herald needs the mail server: set HeraldOptions.Smtp.Host and Port in AddHerald, or choose another transport.")
            .Validate(options => options.Transport != EmailTransportKind.Folder || options.PickupFolder.Length > 0, "Herald needs the folder to write emails to: set HeraldOptions.PickupFolder in AddHerald.")
            .Validate(options => options.Look.BrandColor is ['#', .. var digits] && digits.Length == 6 && digits.All(char.IsAsciiHexDigit), "HeraldOptions.Look.BrandColor must be a color like #2563eb.")
            .Validate(options => options.Retry.MaxAttempts > 0 && options.Retry.InitialDelay > TimeSpan.Zero && options.Retry.MaxDelay >= options.Retry.InitialDelay, "HeraldOptions.Retry needs at least one attempt, a delay above zero and a longest delay that is not shorter.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero, "Herald polls at an interval: set HeraldOptions.PollingInterval to more than zero.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<TemplateStore>();
        services.TryAddSingleton<EmailImages>();
        services.TryAddSingleton<EmailRenderer>();
        services.TryAddSingleton<FailureReasons>();
        services.TryAddSingleton<IEmailTransport>(provider =>
        {
            var options = provider.GetRequiredService<Options.IOptions<HeraldOptions>>();
            return options.Value.Transport == EmailTransportKind.Folder ? new FolderEmailTransport(options) : new SmtpEmailTransport(options);
        });
        services.TryAddSingleton<HeraldEventStore>();
        services.TryAddSingleton<IEmails, Emails>();
        services.TryAddSingleton<EmailDelivery>();
        services.AddHostedService<HeraldWorker>();
        return services;
    }
}
